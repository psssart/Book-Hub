document.addEventListener('DOMContentLoaded', function() {
    // Initialize theme
    initializeTheme();
    
    // Add event listeners to theme toggle buttons
    const themeToggle = document.getElementById('theme-toggle');
    const themeToggleUnauth = document.getElementById('theme-toggle-unauth');
    
    if (themeToggle) {
        themeToggle.addEventListener('click', toggleTheme);
    }
    
    if (themeToggleUnauth) {
        themeToggleUnauth.addEventListener('click', toggleTheme);
    }
});

function initializeTheme() {
    const savedTheme = getCookie('theme') || 'light';
    setTheme(savedTheme);
    updateThemeIcon(savedTheme);
}

function toggleTheme() {
    const currentTheme = getCookie('theme') || 'light';
    const newTheme = currentTheme === 'light' ? 'dark' : 'light';
    
    // Update cookie
    setCookie('theme', newTheme, 365);
    
    // Update theme
    setTheme(newTheme);
    
    // Update icon
    updateThemeIcon(newTheme);
}

function setTheme(theme) {
    document.documentElement.setAttribute('data-theme', theme);
}

function updateThemeIcon(theme) {
    const updateBtn = btn => {
        if (!btn) return;
        const use = btn.querySelector('.theme-icon use');
        if (!use) return;
        
        const symbolId = theme === 'dark' ? 'sun' : 'moon';
        use.setAttribute('href', `/img/svg/theme.svg#${symbolId}`);
        btn.setAttribute('title', theme === 'dark' ? 'Light mode' : 'Dark mode');
    };
    updateBtn(document.getElementById('theme-toggle'));
    updateBtn(document.getElementById('theme-toggle-unauth'));
}

function getCookie(name) {
    const value = `; ${document.cookie}`;
    const parts = value.split(`; ${name}=`);
    if (parts.length === 2) return parts.pop().split(';').shift();
    return null;
}

function setCookie(name, value, days) {
    const expires = new Date(Date.now() + days*864e5).toUTCString();
    document.cookie = `${name}=${value};expires=${expires};path=/;SameSite=Lax`;
}
