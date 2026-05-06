// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


// Functions
function toggleUsername() {
    let currentUsername = document.getElementById("currentUsername");
    let windowSize = window.innerWidth;

    if (windowSize < 805) {
        currentUsername.style.display = "none";
    }
    else {
        currentUsername.style.display = "inline";
    }
}



window.addEventListener("resize", toggleUsername);
document.addEventListener("DOMContentLoaded", () => {
    toggleUsername();
});


