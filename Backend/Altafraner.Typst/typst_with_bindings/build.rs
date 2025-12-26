fn main() -> () {
    csbindgen::Builder::default()
        .input_extern_file("src/lib.rs")
        .csharp_dll_name("typst_with_bindings")
        .generate_csharp_file("Typst.g.cs")
        .unwrap();
}
