use ecow::eco_format;
use typst::diag::StrResult;
use typst::layout::PagedDocument;

pub fn export(
    doc: &PagedDocument,
    standards: &[typst_pdf::PdfStandard],
) -> StrResult<Vec<Vec<u8>>> {
    typst_pdf::pdf(
        doc,
        &typst_pdf::PdfOptions {
            ident: typst::foundations::Smart::Auto,
            standards: typst_pdf::PdfStandards::new(standards).unwrap_or_default(),
            ..Default::default()
        },
    )
    .map(|pdf| vec![pdf])
    .map_err(|e| eco_format!("failed to export PDF: {:?}", e))
}
