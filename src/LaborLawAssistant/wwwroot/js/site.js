// Dark mode button (in _Layout.cshtml): switch between light and dark,
// and remember the choice in the browser (localStorage), so the next page and the next visit use it too.
document.getElementById("themeToggle")?.addEventListener("click", function () {
    var html = document.documentElement;
    var next = html.getAttribute("data-bs-theme") === "dark" ? "light" : "dark";
    html.setAttribute("data-bs-theme", next);
    localStorage.setItem("theme", next);
});
