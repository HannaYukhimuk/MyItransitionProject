document.addEventListener('DOMContentLoaded', function() {
    const savedTheme = getCookie('theme') || 'light';
    document.documentElement.setAttribute('data-bs-theme', savedTheme);
    updateThemeButton(savedTheme);

    const themeToggleBtn = document.getElementById('theme-toggle');
    if (themeToggleBtn) {
        themeToggleBtn.addEventListener('click', toggleTheme);
    }
});

function toggleTheme() {
    const currentTheme = document.documentElement.getAttribute('data-bs-theme');
    const newTheme = currentTheme === 'dark' ? 'light' : 'dark';
    
    document.documentElement.setAttribute('data-bs-theme', newTheme);
    
    document.cookie = `theme=${newTheme};path=/;max-age=${60 * 60 * 24 * 365}`;
    
    updateThemeButton(newTheme);
}

function updateThemeButton(theme) {
    const button = document.getElementById('theme-toggle');
    if (button) {
        if (theme === 'dark') {
            button.innerHTML = '<i class="fas fa-sun"></i>';
            button.title = 'Switch to light theme';
        } else {
            button.innerHTML = '<i class="fas fa-moon"></i>';
            button.title = 'Switch to dark theme';
        }
    }
}

function getCookie(name) {
    const value = `; ${document.cookie}`;
    const parts = value.split(`; ${name}=`);
    if (parts.length === 2) return parts.pop().split(';').shift();
}