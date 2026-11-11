$.validator.addMethod("maxfilesize", function (value, element, maxBytes) {
    return element.files.length === 0 || element.files[0].size <= Number(maxBytes);
});
$.validator.unobtrusive.adapters.addSingleVal("maxfilesize", "max");

$.validator.addMethod("pdffile", function (value, element) {
    return element.files.length === 0 || element.files[0].name.toLowerCase().endsWith(".pdf");
});
$.validator.unobtrusive.adapters.addBool("pdffile");
