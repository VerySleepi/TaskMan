let selectedTaskId = null;
let panelOpen = false;
function toggleCreateTaskPopup() {
    const overlay = document.getElementById('newTask-overlay');
    const content = document.getElementById('newTask-content');

    console.log(overlay)

    // Call CreateTask action to get form HTML and insert it into the popup content, then show the popup
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

    // Set max height of card objects to the sum of the heights of the title and description elements of the tasks
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

function openPanel() {
    const sidePanel = document.getElementById('taskDetailsPanel');
    const content = document.querySelector('.content');
    // Adjust panel behavior based on screen width
    if (window.innerWidth <= 805) {
        sidePanel.classList.add('panel-open');
        content.classList.remove('panel-visible');
    }
    else {
        content.classList.add('panel-visible');
        sidePanel.classList.remove('panel-open');
        sidePanel.classList.remove('hidden');
    }
    panelOpen = true;
}

function closePanel() {
    const sidePanel = document.getElementById('taskDetailsPanel');
    const content = document.querySelector('.content');

    // Close panel and adjust visibility based on screen width
    if (window.innerWidth <= 805) {
        sidePanel.classList.remove('panel-open');
    }
    else {
        sidePanel.classList.add('hidden');
        content.classList.remove('panel-visible');
    }
    panelOpen = false;

}

function toggleTaskDetailPanel() {
    const overlay = document.getElementById('newTask-overlay');
    let sidePanel = document.getElementById('taskDetailsPanel');

    document.querySelectorAll('.taskObject').forEach(card => {
        // Using data attributes to store task details for easy access when a card is clicked
        const taskId = card.dataset.taskId;
        const title = card.dataset.taskTitle;
        const description = card.dataset.taskDescription;
        const dueDate = card.dataset.taskDueDate;
        const status = card.dataset.taskStatus;

        console.log(`Task ID: ${taskId}, Title: ${title}, Status: ${status}`);

        
        if (status === "1") {
            card.classList.add('completed');
        }
      
        card.addEventListener('click', () => {
            document.getElementById('panelTitle').textContent = title;
            document.getElementById('panelDescription').textContent = description;
            document.getElementById('panelDueDate').textContent = "Due-Date: " + dueDate;
            document.getElementById('panelStatus').textContent = status === "1" ? "Completed" : "Not Completed";
            document.getElementById('taskPanelCompleteInput').value = taskId;
            document.getElementById('taskPanelCompleteBtn').textContent = status === "1" ? "Mark Incomplete" : "Complete";
            document.querySelector('.taskPanelEditBtn').dataset.taskId = taskId;


            selectedTaskId = taskId;
            openPanel();
        });

    });
}

function taskListDropdown() {
    // Dropdown for selecting task list when creating a new task
    const dropdown = document.getElementById("taskListDropdown");
    const toggle = document.getElementById("taskListToggle");
    const dropdownList = document.querySelector(".list-dropdown-list");
    const listInput = document.getElementById("taskListID");
    const selectedList = document.getElementById("taskListSelected");
    const dropdownItems = dropdownList.querySelectorAll(".list-dropdown-item button");

    toggle.addEventListener("click", (ev) => {
        ev.preventDefault();
        dropdownList.classList.toggle("hidden");
    });

    dropdownItems.forEach(button => {
        button.addEventListener("click", (ev) => {
            ev.preventDefault();
            listInput.value = button.value;
            selectedList.textContent = button.textContent;
            dropdownList.classList.add("hidden");
        });
    });

    document.addEventListener("click", (ev) => {
        if (!dropdown.contains(ev.target)) {
            dropdownList.classList.add("hidden");
        }
    });
}

function editTask() {
    const taskEditButtons = document.querySelectorAll('.taskEditBtn, .taskPanelEditBtn');
    taskEditButtons.forEach(btn => {
        btn.addEventListener('click', (ev) => {
            ev.stopPropagation();
            const taskId = btn.dataset.taskId;
            const overlay = document.getElementById('newTask-overlay');
            const content = document.getElementById('newTask-content');
            // Fetch pre-filled edit form from EditTask GET action
            fetch(`/Tasks/EditTask?taskId=${taskId}`)
                .then(response => response.text())
                .then(html => {
                    console.log(content);
                    content.innerHTML = html;
                    overlay.classList.remove('hidden');
                })
                .catch(err => console.error("Error loading EditTask form:", err));


        });
    });
}
function deleteTask() {
    let deleteForm = null;
    var taskDeleteBtn = document.querySelectorAll('.taskDeleteBtn');

    taskDeleteBtn.forEach(btn => {
        btn.addEventListener('click', (ev) => {
            ev.stopPropagation();
            // Get form closest to the clicked delete button to later be submitted
            deleteForm = btn.closest('form');
            console.log("Delete btn clicked");
            document.getElementById('deleteTaskOverlay').classList.remove('hidden');
        });
    });


    // Confirm delete button submits the form to delete the task
    const deleteTaskConfirmBtn = document.getElementById('deleteTaskConfirmBtn');
    if (deleteTaskConfirmBtn) {
        deleteTaskConfirmBtn.addEventListener('click', () => {
            deleteForm.submit();
        })
    }

    // Cancel delete button hides the confirmation overlay without deleting
    const deleteTaskCancelBtn = document.getElementById('deleteTaskCancelBtn');
    if (deleteTaskCancelBtn) {
        deleteTaskCancelBtn.addEventListener('click', () => {
            document.getElementById('deleteTaskOverlay').classList.add('hidden');
        })
    }

}
function deleteList() {
    let deleteForm = null;
    var listDeleteBtn = document.querySelectorAll('.listDeleteBtn');

    listDeleteBtn.forEach(btn => {
        btn.addEventListener('click', () => {
            console.log("Delete list btn clicked");
            deleteForm = btn.closest('form');
            document.getElementById('deleteListOverlay').classList.remove('hidden');
        });
    });

    const deleteListConfirmBtn = document.getElementById('deleteListConfirmBtn');
    if (deleteListConfirmBtn) {
        deleteListConfirmBtn.addEventListener('click', () => {
            deleteForm.submit();
        })
    }

    const deleteListCancelBtn = document.getElementById('deleteListCancelBtn');
    if (deleteListCancelBtn) {
        deleteListCancelBtn.addEventListener('click', () => {
            document.getElementById('deleteListOverlay').classList.add('hidden');
        });
    }
}

document.addEventListener('DOMContentLoaded', () => {
    // Side panel is moved off of the screen on smaller screens, allowing for smooth sliding animation. Needs to be visible for this to happen.
    if (window.innerWidth <= 805) {
        document.getElementById('taskDetailsPanel').classList.remove('hidden');
    }

    // Adjust panel visibility and behavior on window resize
    window.addEventListener('resize', () => {
        const sidePanel = document.getElementById('taskDetailsPanel');

        if (panelOpen) {
            // Need to close and reopen panel to adjust classes based on new screen size
            closePanel();
            openPanel();
        }
        else {
            if (window.innerWidth >= 805) {
                sidePanel.classList.add('hidden');
            }
            else {
                sidePanel.classList.remove('hidden');
            }
        }
    });

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

    const listCloseBtn = document.getElementById('newList-close');
    if (listCloseBtn) {
        listCloseBtn.addEventListener('click', () => {
            console.log("Button clicked!");
            toggleCreateListPopup();
        });
    }

    const closeBtn = document.getElementById('newTask-close');
    if (closeBtn) {
        closeBtn.addEventListener('click', () => {
            console.log("Button clicked!");
            toggleCreateTaskPopup();
        });
    }

    const panelCloseBtn = document.querySelector('.panelCloseBtn');
    if (panelCloseBtn) {
        panelCloseBtn.addEventListener('click', () => {
            closePanel();
        });
    }

    var taskStatusCheckboxes = document.querySelectorAll('.task-checkbox');
    taskStatusCheckboxes.forEach(checkbox => {
        checkbox.addEventListener('change', (ev) => {
            ev.stopPropagation();
            checkbox.closest('form').submit();
        });
    });

    taskCardSize();
    toggleTaskDetailPanel();
    deleteTask();
    deleteList();
    editTask();
})


