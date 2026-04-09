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
function taskCardSize() {
    let cardObjects = document.getElementsByClassName("cardObjectList");

    if (cardObjects) {
        console.log("Card objects found: " + cardObjects.length);
        for (let i = 0; i < cardObjects.length; i++) {
            console.log("Processing card object: " + cardObjects[i]);
            let cardObject = cardObjects[i];
            let cardChildren = cardObject.children;
            if (cardChildren.length >= 2) {
                cardObject.style.maxHeight = (parseInt(cardChildren[0].offsetHeight) + parseInt(cardChildren[1].offsetHeight)) + "px";
            }

        }
    }
}

document.addEventListener('DOMContentLoaded', () => {
    
    const newTaskBtn = document.getElementById('newTaskBtn');
    if (newTaskBtn) {
        newTaskBtn.addEventListener('click', () => {
            event.preventDefault();
            toggleCreateTaskPopup();
        })
    }

    const closeBtn = document.getElementById('newTask_close');
    if (closeBtn) {
        closeBtn.addEventListener('click', () => {
            console.log("Button clicked!");
            toggleCreateTaskPopup();
        });
    }
    taskCardSize();
})


