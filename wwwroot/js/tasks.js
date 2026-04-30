let selectedTaskId = null;
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

            document.getElementById('taskTitle').textContent = title;
            document.getElementById('taskDescription').textContent = description;
            document.getElementById('taskDueDate').textContent = dueDate;
            document.getElementById('taskStatus').textContent = status;
            document.getElementById('taskCompletedId').value = taskId;

            selectedTaskId = taskId;

            if (status === 1) {
                var completeBtn = document.querySelector('.completeBtn');
                completeBtn.classList.add('hidden');
            }
            if (sidePanel.classList.contains('hidden')) {
                sidePanel.classList.remove('hidden');
                document.querySelector('.content').classList.add('panel-visible');
            }
        });
        if (card.dataset.taskStatus === "1") {
            card.classList.add('completed');
        }
    });
}

function taskListDropdown() {
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

function deleteTask() {
    var taskDeleteBtn = document.querySelectorAll('.taskDeleteBtn');

    taskDeleteBtn.forEach(btn => {
        btn.addEventListener('click', () => {
            console.log("Delete btn clicked");
            document.getElementById('taskDeleteId').value = selectedTaskId;

            document.getElementById('deleteTaskOverlay').classList.remove('hidden');
        });
    });

}

function deleteTaskOverlay() {
    const deleteTaskConfirmBtn = document.getElementById('deleteTaskConfirmBtn');
    if (deleteTaskConfirmBtn) {
        deleteTaskConfirmBtn.addEventListener('click', () => {
            document.getElementById('deleteTaskForm').submit();
        })
    }

    const deleteTaskCancelBtn = document.getElementById('deleteTaskCancelBtn');
    if (deleteTaskCancelBtn) {
        deleteTaskCancelBtn.addEventListener('click', () => {
            document.getElementById('deleteTaskOverlay').classList.add('hidden');
        })
    }
}

function deleteList() {
    var listDeleteBtn = document.querySelectorAll('.listDeleteBtn');

    listDeleteBtn.forEach(btn => {
        btn.addEventListener('click', () => {
            console.log("Delete list btn clicked");
            const selectedListId = btn.dataset.listId;
            const selectedListName = btn.dataset.listName;
            document.getElementById('listDeleteId').value = selectedListId;

            document.getElementById('deleteListOverlay').classList.remove('hidden');
        });
    });
}

function deleteListOverlay() {
    const deleteListConfirmBtn = document.getElementById('deleteListConfirmBtn');
    if (deleteListConfirmBtn) {
        deleteListConfirmBtn.addEventListener('click', () => {
            document.getElementById('deleteListForm').submit();
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

    const taskEditButtons = document.querySelectorAll('.taskEditBtn');
    taskEditButtons.forEach(btn => {
        btn.addEventListener('click', () => {
            // Fetch pre-filled edit form from EditTask GET action
            fetch(`/Tasks/EditTask?taskId=${selectedTaskId}`)
                .then(response => response.text())
                .then(html => {
                    content.innerHTML = html;
                    overlay.classList.remove('hidden');
                })
                .catch(err => console.error("Error loading EditTask form:", err));


        });
    });

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

    const panelCloseBtn = document.getElementById('panelCloseBtn');
    if (panelCloseBtn) {
        panelCloseBtn.addEventListener('click', () => {
            const sidePanel = document.getElementById('taskDetailsPanel');
            const content = document.querySelector('.content');
            sidePanel.classList.add('hidden');
            content.classList.remove('panel-visible');
        });
    }


    taskCardSize();
    toggleTaskDetailPanel();
    deleteTask();
    deleteList();
    deleteTaskOverlay();
    deleteListOverlay();
})


