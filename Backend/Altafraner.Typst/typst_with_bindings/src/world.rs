use std::collections::HashMap;
use std::fs;
use std::path::PathBuf;
use std::sync::{Arc, Mutex, OnceLock};

use chrono::{DateTime, Datelike, Local};
use log::{debug, info};
use std::fmt::Display;

use typst::diag::{FileError, FileResult, StrResult};
use typst::foundations::{Bytes, Datetime};
use typst::syntax::{FileId, Source, VirtualPath};
use typst::text::{Font, FontBook};
use typst::utils::LazyHash;
use typst::{Library, LibraryExt, World};

use typst_kit::download::{DownloadState, Downloader, Progress};
use typst_kit::{fonts::FontSearcher, package::PackageStorage};

pub struct LoggingDownload<T>(pub T);

impl<T: Display> Progress for LoggingDownload<T> {
    fn print_start(&mut self) {
        info!("Downloading: {}", self.0);
    }

    fn print_progress(&mut self, state: &DownloadState) {
        let total = state.content_len.unwrap_or(state.total_downloaded);
        let percent = state.total_downloaded as f32 / total as f32 * 100.0;
        debug!(
            "Progress for {}: {:.1}% ({} / {} bytes)",
            self.0, percent, state.total_downloaded, total
        );
    }

    fn print_finish(&mut self, state: &DownloadState) {
        info!(
            "Finished downloading {} ({} bytes total)",
            self.0, state.total_downloaded
        );
    }
}

pub struct TypstWorld {
    main: FileId,
    library: LazyHash<Library>,
    book: LazyHash<FontBook>,
    fonts: Arc<typst_kit::fonts::Fonts>,
    slots: Mutex<HashMap<FileId, LoadedFile>>,
    resolver: FileResolver,
    now: OnceLock<DateTime<Local>>,
    virtual_files: Mutex<HashMap<VirtualPath, Vec<u8>>>,
}

impl World for TypstWorld {
    fn library(&self) -> &LazyHash<Library> {
        &self.library
    }
    fn book(&self) -> &LazyHash<FontBook> {
        &self.book
    }
    fn main(&self) -> FileId {
        self.main
    }

    fn source(&self, id: FileId) -> FileResult<Source> {
        self.with_slot(id, |slot| slot.load_source(&self.resolver))
    }

    fn file(&self, id: FileId) -> FileResult<Bytes> {
        let vpath = id.vpath();

        if let Some(bytes) = self.virtual_files.lock().unwrap().get(vpath).cloned() {
            return Ok(Bytes::new(bytes));
        }

        self.with_slot(id, |slot| slot.load_bytes(&self.resolver))
    }

    fn font(&self, index: usize) -> Option<Font> {
        self.fonts.fonts[index].get()
    }

    fn today(&self, offset: Option<i64>) -> Option<Datetime> {
        let now = self.now.get_or_init(Local::now);
        let n = match offset {
            None => now.naive_local(),
            Some(o) => now.naive_utc() + chrono::Duration::hours(o),
        };
        Datetime::from_ymd(
            n.year(),
            n.month().try_into().ok()?,
            n.day().try_into().ok()?,
        )
    }
}

impl TypstWorld {
    pub fn new(
        root: PathBuf,
        font_paths: &[PathBuf],
        input_content: Option<String>,
        include_system_fonts: bool,
    ) -> StrResult<Self> {
        let main = FileId::new_fake(VirtualPath::new("<main>"));
        let mut slots = HashMap::new();

        if let Some(text) = input_content {
            let mut lf = LoadedFile::new(main);
            lf.source_cache.init(Source::new(main, text));
            slots.insert(main, lf);
        }

        let fonts = {
            let mut searcher = FontSearcher::new();
            searcher.include_system_fonts(include_system_fonts);
            searcher.search_with(font_paths)
        };

        Ok(Self {
            resolver: FileResolver::new(
                root,
                PackageStorage::new(None, None, Downloader::new("typst")),
            ),
            main,
            library: LazyHash::new(typst::Library::builder().build()),
            book: LazyHash::new(fonts.book.clone()),
            fonts: Arc::new(fonts),
            slots: Mutex::new(slots),
            now: OnceLock::new(),
            virtual_files: Mutex::new(HashMap::new()),
        })
    }

    pub fn set_inputs(&self, inputs: &str) {
        let mut vf = self.virtual_files.lock().unwrap();
        vf.insert(VirtualPath::new("inputs.json"), inputs.as_bytes().to_vec());
    }

    fn with_slot<F, T>(&self, id: FileId, f: F) -> T
    where
        F: FnOnce(&mut LoadedFile) -> T,
    {
        let mut map = self.slots.lock().unwrap();
        let slot = map.entry(id).or_insert_with(|| LoadedFile::new(id));
        f(slot)
    }
}

struct FileResolver {
    root: PathBuf,
    packages: PackageStorage,
}

impl FileResolver {
    fn new(root: PathBuf, packages: PackageStorage) -> Self {
        Self { root, packages }
    }

    fn resolve_path(&self, id: FileId) -> FileResult<PathBuf> {
        let mut base = &self.root;
        let tmp;
        if let Some(spec) = id.package() {
            tmp = self
                .packages
                .prepare_package(spec, &mut LoggingDownload(&spec))?;
            base = &tmp;
        }
        id.vpath().resolve(base).ok_or(FileError::AccessDenied)
    }

    fn read_bytes(&self, id: FileId) -> FileResult<Vec<u8>> {
        let path = self.resolve_path(id)?;
        let f = |e| FileError::from_io(e, &path);
        if fs::metadata(&path).map_err(f)?.is_dir() {
            Err(FileError::IsDirectory)
        } else {
            fs::read(&path).map_err(f)
        }
    }
}

struct LoadedFile {
    id: FileId,
    source_cache: SourceCache,
    bytes_cache: BytesCache,
}

impl LoadedFile {
    fn new(id: FileId) -> Self {
        Self {
            id,
            source_cache: SourceCache::new(),
            bytes_cache: BytesCache::new(),
        }
    }

    fn load_source(&mut self, resolver: &FileResolver) -> FileResult<Source> {
        self.source_cache.compute(self.id, resolver)
    }

    fn load_bytes(&mut self, resolver: &FileResolver) -> FileResult<Bytes> {
        self.bytes_cache.compute(self.id, resolver)
    }
}

struct SourceCache {
    data: Option<FileResult<Source>>,
    fingerprint: u128,
    set: bool,
}

impl SourceCache {
    fn new() -> Self {
        Self {
            data: None,
            fingerprint: 0,
            set: false,
        }
    }

    fn init(&mut self, s: Source) {
        self.data = Some(Ok(s));
        self.set = true;
    }

    fn compute(&mut self, id: FileId, resolver: &FileResolver) -> FileResult<Source> {
        if self.set {
            if let Some(x) = &self.data {
                return x.clone();
            }
        }
        self.set = true;

        let raw = resolver.read_bytes(id);
        let new_fp = typst::utils::hash128(&raw);

        if self.fingerprint == new_fp {
            if let Some(x) = &self.data {
                return x.clone();
            }
        }
        self.fingerprint = new_fp;

        let prev = self.data.take().and_then(Result::ok);
        let new = raw.and_then(|b| {
            let text = std::str::from_utf8(&b)?;
            Ok(match prev {
                Some(mut s) => {
                    s.replace(text);
                    s
                }
                None => Source::new(id, text.into()),
            })
        });

        self.data = Some(new.clone());
        new
    }
}

struct BytesCache {
    data: Option<FileResult<Bytes>>,
    fingerprint: u128,
    set: bool,
}

impl BytesCache {
    fn new() -> Self {
        Self {
            data: None,
            fingerprint: 0,
            set: false,
        }
    }

    fn compute(&mut self, id: FileId, resolver: &FileResolver) -> FileResult<Bytes> {
        if self.set {
            if let Some(x) = &self.data {
                return x.clone();
            }
        }
        self.set = true;

        let raw = resolver.read_bytes(id);
        let new_fp = typst::utils::hash128(&raw);

        if self.fingerprint == new_fp {
            if let Some(x) = &self.data {
                return x.clone();
            }
        }
        self.fingerprint = new_fp;

        let new = raw.and_then(|b| Ok(Bytes::new(b)));
        self.data = Some(new.clone());
        new
    }
}
