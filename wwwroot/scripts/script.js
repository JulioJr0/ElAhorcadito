// script "script.js"
const token = localStorage.getItem('jwtToken');
if (!token) {
    window.location.href = 'login.html';
}

const temaSeleccionado = JSON.parse(sessionStorage.getItem('temaSeleccionado'));
console.log(temaSeleccionado);
if (!temaSeleccionado) {
    alert('No se seleccionó ningún tema');
    window.location.href = 'temas.html';
}

const hangmanImage = document.querySelector(".hangman-box img");
const wordDisplay = document.querySelector(".word-display");
const guessesText = document.querySelector(".guesses-text b");
const keyboardDiv = document.querySelector(".keyboard");
const gameModal = document.querySelector(".game-modal");
const playAgainBtn = document.querySelector(".play-again");

let currentWord, correctLetters, wrongGuessCount;
const maxGuesses = 6;
let palabrasCifradas = [];
let palabraIndex = 0;

function descifrarPalabra(palabraCifrada) {
    try {
        return atob(palabraCifrada);
    } catch (e) {
        console.error('Error descifrando palabra:', e);
        return '';
    }
}

async function cargarPalabrasTema() {
    try {
        palabrasCifradas = temaSeleccionado.palabrasCifradas || [];
        palabraIndex = temaSeleccionado.palabrasCompletadas || 0;

        if (palabraIndex >= palabrasCifradas.length) {
            alert('¡Ya completaste todas las palabras de este tema!');
            window.location.href = 'temas.html';
            return;
        }

        currentWord = descifrarPalabra(palabrasCifradas[palabraIndex]);
        console.log(currentWord);
        resetGame();
    } catch (error) {
        console.error('Error cargando palabras:', error);
        alert('Error al cargar el juego');
    }
}

const resetGame = () => {
    correctLetters = [];
    wrongGuessCount = 0;
    hangmanImage.src = `images/hangman-${wrongGuessCount}.svg`;
    guessesText.innerText = `${wrongGuessCount} / ${maxGuesses}`;
    keyboardDiv.querySelectorAll("button").forEach(btn => btn.disabled = false);
    wordDisplay.innerHTML = currentWord.split("").map(() => `<li class="letter"></li>`).join("");
    gameModal.classList.remove("show");
}

const gameOver = async (isVictory) => {
    setTimeout(async () => {
        const modalText = isVictory ? `You found the word:` : `The correct word was:`;
        gameModal.querySelector("img").src = `images/${isVictory ? `victory` : `lost`}.gif`;
        gameModal.querySelector("h4").innerText = `${isVictory ? `Congrats` : `Game Over`}!`;
        gameModal.querySelector("p").innerHTML = `${modalText} <b>${currentWord}</b>`;
        gameModal.classList.add("show");

        if (isVictory) {
            await actualizarProgreso('GANADA');
        }
    }, 300);
}

async function actualizarProgreso(resultado) {
    try {
        const response = await fetch('/api/Juego/actualizar-progreso', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}`
            },
            body: JSON.stringify({
                idTema: temaSeleccionado.idTema,
                palabraActualIndex: palabraIndex,
                resultado: resultado
            })
        });

        if (response.ok) {
            console.log('Progreso actualizado');
            palabraIndex++;
        } else if (response.status === 401) {
            localStorage.removeItem('jwtToken');
            window.location.href = 'login.html';
        }
    } catch (error) {
        console.error('Error actualizando progreso:', error);
    }
}

const initGame = (button, clickedLetter) => {
    if (currentWord.includes(clickedLetter)) {
        [...currentWord].forEach((letter, index) => {
            if (letter === clickedLetter) {
                correctLetters.push(letter);
                wordDisplay.querySelectorAll("li")[index].innerText = letter;
                wordDisplay.querySelectorAll("li")[index].classList.add("guessed");
            }
        });
    } else {
        wrongGuessCount++;
        hangmanImage.src = `images/hangman-${wrongGuessCount}.svg`;
    }

    button.disabled = true;
    guessesText.innerText = `${wrongGuessCount} / ${maxGuesses}`;

    if (wrongGuessCount === maxGuesses) return gameOver(false);
    if (correctLetters.length === currentWord.length) return gameOver(true);
}

for (let i = 97; i <= 110; i++) {
    const char = String.fromCharCode(i);
    const button = document.createElement("button");
    button.innerText = char;
    keyboardDiv.appendChild(button);
    button.addEventListener("click", e => initGame(e.target, char));
}

const buttonNtilde = document.createElement("button");
buttonNtilde.innerText = "ñ";
keyboardDiv.appendChild(buttonNtilde);
buttonNtilde.addEventListener("click", e => initGame(e.target, "ñ"));

for (let i = 111; i <= 122; i++) {
    const char = String.fromCharCode(i);
    const button = document.createElement("button");
    button.innerText = char;
    keyboardDiv.appendChild(button);
    button.addEventListener("click", e => initGame(e.target, char));
}

playAgainBtn.addEventListener("click", async function () {
    if (palabraIndex < palabrasCifradas.length) {
        currentWord = descifrarPalabra(palabrasCifradas[palabraIndex]);
        resetGame();
    } else {
        alert('¡Completaste todas las palabras de este tema!');
        window.location.href = 'temas.html';
    }
});

cargarPalabrasTema();