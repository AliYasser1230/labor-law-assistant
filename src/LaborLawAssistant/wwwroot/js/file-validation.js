// Browser-side versions of our C# validation attributes (Validation/MaxFileSizeAttribute.cs, PdfFileAttribute.cs).
// They show the error message immediately, before the file is uploaded.
// The server checks again, because browser checks can be turned off.

// [MaxFileSize]: reads data-val-maxfilesize-max (the limit in bytes) from the <input>.
$.validator.addMethod("maxfilesize", function (value, element, maxBytes) {
    return element.files.length === 0 || element.files[0].size <= Number(maxBytes);
});
$.validator.unobtrusive.adapters.addSingleVal("maxfilesize", "max");

// [PdfFile]: the file name must end with ".pdf".
$.validator.addMethod("pdffile", function (value, element) {
    return element.files.length === 0 || element.files[0].name.toLowerCase().endsWith(".pdf");
});
$.validator.unobtrusive.adapters.addBool("pdffile");
