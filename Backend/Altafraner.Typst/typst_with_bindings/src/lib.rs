#![allow(non_camel_case_types)]
use std::ffi::{c_char, CStr, CString};
use std::path::PathBuf;
use std::ptr;

mod compiler;
mod world;

use ecow::EcoString;
use typst::diag::{StrResult, Warned};
use typst::layout::PagedDocument;
use world::TypstWorld;

pub struct Compiler {
    pub state: TypstWorld,
}

#[repr(C)]
#[derive(Clone, Copy)]
pub struct Buffer {
    pub ptr: *mut u8,
    pub len: usize,
}

#[repr(C)]
pub struct Warning {
    pub message: *mut c_char,
}

#[repr(C)]
pub struct CompileResult {
    pub buffers: *mut Buffer,
    pub buffers_len: usize,
    pub error: *mut c_char,
}

impl Default for CompileResult {
    fn default() -> Self {
        Self {
            buffers: ptr::null_mut(),
            buffers_len: 0,
            error: ptr::null_mut(),
        }
    }
}

unsafe fn cstr_to_str<'a>(ptr: *const c_char, default: &'a str) -> &'a str {
    if ptr.is_null() {
        return default;
    }
    match CStr::from_ptr(ptr).to_str() {
        Ok(s) if !s.is_empty() => s,
        _ => default,
    }
}

unsafe fn cstr_slice<'a>(ptrs: *const *const c_char, len: usize) -> &'a [*const c_char] {
    if ptrs.is_null() || len == 0 {
        return &[];
    }
    std::slice::from_raw_parts(ptrs, len)
}

#[unsafe(no_mangle)]
pub extern "C" fn create_compiler(
    root: *const c_char,
    input_source: *const c_char,
    font_paths: *const *const c_char,
    font_paths_len: usize,
    ignore_system_fonts: bool,
) -> *mut Compiler {
    let root = PathBuf::from(unsafe { cstr_to_str(root, ".") });
    let input_content = unsafe { cstr_to_str(input_source, "") };
    let input_content = if input_content.is_empty() {
        None
    } else {
        Some(input_content.to_string())
    };

    let font_paths: Vec<PathBuf> = unsafe {
        cstr_slice(font_paths, font_paths_len)
            .iter()
            .map(|&p| PathBuf::from(cstr_to_str(p, "")))
            .collect()
    };

    match TypstWorld::new(root, &font_paths, input_content, !ignore_system_fonts) {
        Ok(world) => Box::into_raw(Box::new(Compiler { state: world })),
        Err(_) => ptr::null_mut(),
    }
}

#[unsafe(no_mangle)]
pub extern "C" fn free_compiler(ptr: *mut Compiler) {
    if !ptr.is_null() {
        unsafe { drop(Box::from_raw(ptr)) }
    }
}

#[unsafe(no_mangle)]
pub extern "C" fn set_sys_inputs(ptr: *mut Compiler, sys_inputs: *const c_char) -> bool {
    if ptr.is_null() {
        return false;
    }
    let compiler = unsafe { &mut *ptr };

    let inputs = unsafe { cstr_to_str(sys_inputs, "{}") };
    compiler.state.set_inputs(inputs);
    true
}

fn compile_inner(world: &mut TypstWorld) -> StrResult<Vec<Vec<u8>>> {
    let doc = match typst::compile::<PagedDocument>(world) {
        Warned { output, .. } => output.map_err(|e| EcoString::from(format!("{:?}", e)))?,
    };
    compiler::export(&doc, &[])
}

#[unsafe(no_mangle)]
pub extern "C" fn compile(ptr: *mut Compiler) -> CompileResult {
    if ptr.is_null() {
        return CompileResult {
            error: CString::new("compiler ptr was null").unwrap().into_raw(),
            ..Default::default()
        };
    }

    let compiler = unsafe { &mut *ptr };
    match compile_inner(&mut compiler.state) {
        Ok(buffers) => {
            let mut out: Vec<Buffer> = buffers
                .into_iter()
                .map(|mut v| {
                    v.shrink_to_fit();
                    let b = Buffer {
                        ptr: v.as_mut_ptr(),
                        len: v.len(),
                    };
                    std::mem::forget(v);
                    b
                })
                .collect();

            out.shrink_to_fit();

            let result = CompileResult {
                buffers: out.as_mut_ptr(),
                buffers_len: out.len(),
                error: ptr::null_mut(),
            };

            std::mem::forget(out);
            result
        }
        Err(err) => {
            let e = CString::new(err.to_string()).unwrap().into_raw();
            CompileResult {
                error: e,
                ..Default::default()
            }
        }
    }
}

#[unsafe(no_mangle)]
pub extern "C" fn free_compile_result(result: CompileResult) {
    unsafe {
        if !result.buffers.is_null() {
            let buffers =
                Vec::from_raw_parts(result.buffers, result.buffers_len, result.buffers_len);
            for b in buffers {
                if !b.ptr.is_null() {
                    drop(Vec::from_raw_parts(b.ptr, b.len, b.len))
                }
            }
        }
        if !result.error.is_null() {
            drop(CString::from_raw(result.error))
        }
    }
}

#[unsafe(no_mangle)]
pub extern "C" fn free_string(s: *mut c_char) {
    if !s.is_null() {
        unsafe { drop(CString::from_raw(s)) }
    }
}
