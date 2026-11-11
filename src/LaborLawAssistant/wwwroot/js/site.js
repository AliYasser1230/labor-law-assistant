document.getElementById("themeToggle")?.addEventListener("click", function () {
    var html = document.documentElement;
    var next = html.getAttribute("data-bs-theme") === "dark" ? "light" : "dark";
    html.setAttribute("data-bs-theme", next);
    localStorage.setItem("theme", next);
});
