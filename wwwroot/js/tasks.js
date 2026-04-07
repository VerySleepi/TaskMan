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