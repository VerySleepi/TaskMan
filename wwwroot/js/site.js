// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


// Functions
function toggleNavbar() {
    var width = window.innerWidth;
    const accountNav = document.getElementById('account-nav');

    if (accountNav == null) {
        return;
    }

    if (width < 576 && (accountNav.classList.contains("ms-auto" && "align-items-center") == true)) {
        accountNav.classList.toggle('ms-auto');
        accountNav.classList.toggle('align-items-center');
        console.dir(accountNav.classList);
        console.log(accountNav.classList.contains("ms-auto" && "align-items-center"));
    }
    else {
        if (width >= 576 && (accountNav.classList.contains("ms-auto" && "align-items-center") == false)) {
            accountNav.classList.toggle('ms-auto');
            accountNav.classList.toggle('align-items-center');
            console.log(accountNav.classList);
            console.log(width);
            console.log(accountNav.classList.contains("ms-auto" && "align-items-center"));
        }
    }
};

window.addEventListener('resize', toggleNavbar);
window.addEventListener('DOMContentLoaded', toggleNavbar);