if (!window.__themeToggleInit) {
    window.__themeToggleInit = true;

    document.addEventListener("DOMContentLoaded", () => {
        initializeTheme();

        document.addEventListener("click", (e) => {
            const ctl = e.target.closest(".theme-toggle-btn, .theme-toggle-link, .theme-toggle-ctl");
            if (!ctl) return;
            e.preventDefault();
            toggleTheme();
        });
    });

    function initializeTheme() {
        const savedTheme = getCookie("theme") || "light";
        setTheme(savedTheme);
        updateThemeIcons(savedTheme);
    }

    function toggleTheme() {
        const currentTheme = getCookie("theme") || "light";
        const newTheme = currentTheme === "light" ? "dark" : "light";
        setCookie("theme", newTheme, 365);
        setTheme(newTheme);
        updateThemeIcons(newTheme);
    }

    function setTheme(theme) {
        document.documentElement.setAttribute("data-theme", theme);
    }

    function updateThemeIcons(theme) {
        const symbolId = theme === "dark" ? "sun" : "moon";
        
        document.querySelectorAll(".theme-icon use").forEach((use) => {
            use.setAttribute("href", `#${symbolId}`);
            use.setAttribute("xlink:href", `#${symbolId}`);
        });
    }

    function getCookie(name) {
        const value = `; ${document.cookie}`;
        const parts = value.split(`; ${name}=`);
        if (parts.length === 2) return parts.pop().split(";").shift();
        return null;
    }

    function setCookie(name, value, days) {
        const expires = new Date(Date.now() + days * 864e5).toUTCString();
        document.cookie = `${name}=${value};expires=${expires};path=/;SameSite=Lax`;
    }
}
