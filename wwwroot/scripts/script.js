const hangmanImage = document.querySelector(".hangman-box img");
const wordDisplay = document.querySelector(".word-display");
const guessesText = document.querySelector(".guesses-text b");
const keyboardDiv = document.querySelector(".keyboard");
const gameModal = document.querySelector(".game-modal");
const playAgainBtn = document.querySelector(".play-again");

let currentWord, correctLetters, wrongGuessCount;
const maxGuesses = 6;

const resetGame = () => {
    //Reinicia todas las variables del juego y los elementos UI
    correctLetters = [];
    wrongGuessCount = 0;
    hangmanImage.src = `images/hangman-${wrongGuessCount}.svg`;
    // guessesText.innerText = `${wrongGuessCount} / ${maxGuesses}`;
    keyboardDiv.querySelectorAll("button").forEach(btn => btn.disabled = false);
    wordDisplay.innerHTML = currentWord.split("").map(() => `<li class="letter"></li>`).join("");
    gameModal.classList.remove("show");
}

const getRandomWord = () => {
    //selecciona una palabra aleatoria y pista de wordList
    const {word, hint } = wordList[Math.floor(Math.random() * wordList.length)];
    currentWord = word;
    // document.querySelector(".hint-text b").innerText = hint; //tema //me faltaba el punto de la clase "." para localizar el elemento
    resetGame();
}

const gameOver = (isVictory) => {
    
    //Después de 300ms del juego completado.. Muestra el modal con datos relevantes
    setTimeout(() => {
        const modalText = isVictory?`You found the word:`:`The correct word was:`;
        gameModal.querySelector("img").src = `images/${isVictory?`victory`:`lost`}.gif`;
        gameModal.querySelector("h4").innerText = `${isVictory?`Congrats!`:`Game Over!`}`;
        gameModal.querySelector("p").innerHTML = `${modalText} <b>${currentWord}</b>`;
        gameModal.classList.add("show");
    }, 300);
}

const initGame = (button, clickedLetter) => {
    //Revisa si clickedLetter existe en currentWord
    if (currentWord.includes(clickedLetter)) {
        //Muestra todas las letras correctas en la palabra display
        [...currentWord].forEach((letter, index) => {
            if (letter === clickedLetter) {
                correctLetters.push(letter)
               wordDisplay.querySelectorAll("li")[index].innerText = letter; 
               wordDisplay.querySelectorAll("li")[index].classList.add("guessed"); 
            }
        });
    }else{
        //Si das click en una letra que no existe entonces actualizo wrongGuessCount y la imagen hangman
        wrongGuessCount++;
        hangmanImage.src = `images/hangman-${wrongGuessCount}.svg`;
    }

    button.disabled = true;
    guessesText.innerText = `${wrongGuessCount} / ${maxGuesses}`;

    //Llama la función gameOver si alguna de las condiciones entra. 
    if (wrongGuessCount === maxGuesses) return gameOver(false);
    if (correctLetters.length === currentWord.length) return gameOver(true);
}
// Creando los botones del teclado
for (let i = 97; i <= 110; i++) {
    const char = String.fromCharCode(i);
    const button = document.createElement("button");
    button.innerText = char;
    keyboardDiv.appendChild(button);
    button.addEventListener("click", e => initGame(e.target, char));
}

const buttonNtilde = document.createElement("button");
const ntilde = "ñ"; // La letra "ñ"
buttonNtilde.innerText = ntilde;
keyboardDiv.appendChild(buttonNtilde);
buttonNtilde.addEventListener("click", e => initGame(e.target, ntilde));

for (let i = 111; i <= 122; i++) {
    const char = String.fromCharCode(i);
    const button = document.createElement("button");
    button.innerText = char;
    keyboardDiv.appendChild(button);
    button.addEventListener("click", e => initGame(e.target, char));
}

getRandomWord();
playAgainBtn.addEventListener("click", getRandomWord);