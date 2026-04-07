var textInput = document.getElementById('input-text');
var userInput = document.getElementById('input-username');
var inputBtn = document.getElementById('input-btn');
textInput.addEventListener('input', function (e) {
    updateButton()
})
userInput.addEventListener('input', function (e) {
    updateButton()
})
function updateButton() {
    inputBtn.disabled = textInput.value === "" || userInput.value === "";
}
updateButton()