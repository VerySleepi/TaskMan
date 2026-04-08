(() => {
    checkFunctionality();
}
)();

function checkFunctionality() {
    var buttons = document.getElementsByClassName('newTaskBtn');

    for (var i = 0; i < buttons.length; i++) {
        buttons[i].addEventListener("click", function () {
            console.log("Button Clicked!");
        })
    }
}

function toggleCreateTaskPopup() {
    const overlay = document.getElementById('newTask-overlay');
    const content = document.getElementById('newTask-content');

    if (overlay.classList.contains('hidden')) {
        fetch("/Tasks/CreateTask")
            .then(response => response.text())
            .then(html => {
                content.innerHTML = html;
                overlay.classList.remove('hidden');
            })
            .catch(err => console.error("Error loading CreateTask form:", err))
    } else {
        overlay.classList.add('hidden');
        content.innerHTML = '';
    }
}

document.addEventListener('DOMContentLoaded', () => {
    const newTaskBtn = document.getElementById('newTaskBtn');
    if (newTaskBtn) {
        newTaskBtn.addEventListener('click', toggleCreateTaskPopup)
    }

    const closeBtn = document.getElementById('newTask-close');
    if (closeBtn) {
        closeBtn.addEventListener('click', toggleCreateTaskPopup);
    }
})
