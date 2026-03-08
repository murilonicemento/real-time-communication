"use strict";

// Cria uma conexão com o Hub do SignalR.
// "/chatHub" precisa ser o mesmo endpoint configurado no Program.cs ou Startup.
var connection = new signalR.HubConnectionBuilder()
    .withUrl("/chatHub")
    .build();

// Desabilita o botão de envio enquanto a conexão não estiver estabelecida
document.getElementById("sendButton").disabled = true;

// Evento que será executado quando o servidor chamar "ReceiveMessage"
// Esse nome precisa ser exatamente igual ao usado no SendAsync do Hub.
connection.on("ReceiveMessage", function (user, message) {

    // Cria um novo elemento <li> para exibir a mensagem
    var li = document.createElement("li");

    // Adiciona o elemento na lista de mensagens
    document.getElementById("messagesList").appendChild(li);

    // textContent evita problemas de XSS pois não interpreta HTML
    li.textContent = `${user} says ${message}`;
});

// Evento chamado quando alguém entra ou sai de um grupo
connection.on("Send", function (message) {

    var li = document.createElement("li");

    document.getElementById("messagesList").appendChild(li);

    li.textContent = message;
});

// Inicia a conexão com o servidor
connection.start()
    .then(function () {

        // Após conectar, habilita o botão de envio
        document.getElementById("sendButton").disabled = false;

    })
    .catch(function (err) {

        // Caso a conexão falhe, loga o erro
        return console.error(err.toString());

    });


// Evento do botão de envio de mensagem privada
document.getElementById("sendButton").addEventListener("click", function (event) {

    // Obtém o usuário de destino
    var user = document.getElementById("userInput").value;

    // Obtém o conteúdo da mensagem
    var message = document.getElementById("messageInput").value;

    // Chama o método do Hub "SendPrivateMessage"
    // invoke executa métodos no servidor
    connection.invoke("SendPrivateMessage", user, message)
        .catch(function (err) {
            return console.error(err.toString());
        });

    // Evita que o formulário recarregue a página
    event.preventDefault();
});


// Evento para entrar em um grupo
document.getElementById("joinGroup").addEventListener("click", function () {

    // Nome do grupo que o usuário quer entrar
    var group = document.getElementById("groupInput").value;

    // Chama o método AddToGroup no Hub
    connection.invoke("AddToGroup", group)
        .catch(function (err) {
            return console.error(err.toString());
        });
});


// Evento para sair de um grupo
document.getElementById("leaveGroup").addEventListener("click", function () {

    var group = document.getElementById("groupInput").value;

    // Chama o método RemoveFromGroup no Hub
    connection.invoke("RemoveFromGroup", group)
        .catch(function (err) {
            return console.error(err.toString());
        });
});