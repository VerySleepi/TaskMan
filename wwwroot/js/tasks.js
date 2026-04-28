let selectedTaskId = null;
function toggleCreateTaskPopup() {
    const overlay = document.getElementById('newTask-overlay');
    const content = document.getElementById('newTask-content');

    console.log(overlay)

    if (overlay.classList.contains('hidden')) {
        fetch("/Tasks/CreateTask")
            .then(response => response.text())
            .then(html => {
                content.innerHTML = html;
                overlay.classList.remove('hidden');
                taskListDropdown();
            })
            .catch(err => console.error("Error loading CreateTask form:", err))
    } else {
        overlay.classList.add('hidden');
        content.innerHTML = '';
    }
}

function toggleCreateListPopup() {
    const overlay = document.getElementById('newList-overlay');
    const content = document.getElementById('newList-content');

    console.log(overlay)

    if (overlay.classList.contains('hidden')) {
        fetch("/Tasks/CreateList")
            .then(response => response.text())
            .then(html => {
                content.innerHTML = html;            
                overlay.classList.remove('hidden');
            })
            .catch(err => console.error("Error loading CreateList form:", err))
    } else {
        overlay.classList.add('hidden');
        content.innerHTML = '';
    }
}

function taskCardSize() {
    let cardObjects = document.getElementsByClassName("cardObjectList");

    // Set max heigh of card objects to the sum of the heights of the title and description elements of the tasks
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

function toggleTaskDetailPanel() {
    const overlay = document.getElementById('newTask-overlay');
    const content = document.getElementById('newTask-content');
    let sidePanel = document.getElementById('taskDetailsPanel');

    document.querySelectorAll('.taskObject').forEach(card => {
        card.addEventListener('click', () => {

            const taskId = card.dataset.taskId;
            const title = card.dataset.taskTitle;
            const description = card.dataset.taskDescription;
            const dueDate = card.dataset.taskDueDate;
            const status = card.dataset.taskStatus;

            console.log("Task ID: " + taskId);
            console.log("Title: " + title);
            console.log("Description: " + description);

            document.getElementById('taskTitle').textContent = title;
            console.log("Title: " + title);
            document.getElementById('taskDescription').textContent = description;
            console.log("Description: " + description);
            document.getElementById('taskDueDate').textContent = dueDate;
            console.log("Due Date: " + dueDate);
            document.getElementById('taskStatus').textContent = status;
            console.log("Status: " + status);

            document.getElementById('taskCompletedId').value = taskId;

            selectedTaskId = taskId;

            if (sidePanel.classList.contains('hidden')) {
                sidePanel.classList.remove('hidden');
            }

        });
    });

    document.getElementById('taskEditBtn').addEventListener('click', () => {
        // Fetch pre-filled edit form from EditTask GET action
        fetch(`/Tasks/EditTask?taskId=${selectedTaskId}`)
            .then(response => response.text())
            .then(html => {
                content.innerHTML = html;
                overlay.classList.remove('hidden');
            })
            .catch(err => console.error("Error loading EditTask form:", err));

    });


}

function taskListDropdown() {
    const dropdown = document.getElementById("taskListDropdown");
    const toggle = document.getElementById("taskListToggle");
    const dropdownList = document.querySelector(".list-dropdown-list");
    const listInput = document.getElementById("taskListID");
    const selectedList = document.getElementById("taskListSelected");
    const dropdownItems = dropdownList.querySelectorAll(".list-dropdown-item");

    toggle.addEventListener("click", (ev) => {
        ev.preventDefault();
        dropdownList.classList.toggle("hidden");
    });

    dropdownItems.forEach(item => {
        item.addEventListener("click", (ev) => {
            ev.preventDefault();
            listInput.value = item.value;
            selectedList.textContent = item.textContent;
            dropdownList.classList.add("hidden");
        });
    });

    document.addEventListener("click", (ev) => {
        if (!dropdown.contains(ev.target)) {
            dropdownList.classList.add("hidden");
        }
    });
}

document.addEventListener('DOMContentLoaded', () => {
    
    const newTaskBtn = document.getElementById('newTaskBtn');
    if (newTaskBtn) {
        newTaskBtn.addEventListener('click', () => {
            event.preventDefault();
            toggleCreateTaskPopup();
        })
    }

    const newListBtn = document.getElementById('newListBtn');
    if (newListBtn) {
        newListBtn.addEventListener('click', () => {
            console.log("Button clicked!");
            event.preventDefault();
            toggleCreateListPopup();
        })
    }

    const listCloseBtn = document.getElementById('newList_close');
    if (listCloseBtn) {
        listCloseBtn.addEventListener('click', () => {
            console.log("Button clicked!");
            toggleCreateListPopup();
        });
    }

    const closeBtn = document.getElementById('newTask_close');
    if (closeBtn) {
        closeBtn.addEventListener('click', () => {
            console.log("Button clicked!");
            toggleCreateTaskPopup();
        });
    }
    taskCardSize();
    toggleTaskDetailPanel();
})


