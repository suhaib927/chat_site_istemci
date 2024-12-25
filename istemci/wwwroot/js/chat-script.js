let currentChatId = null;
function selectChat(element) {
    const chatId = element;
    currentChatId = element;

    fetch(`/Chat/LoadChat?chatId=${chatId.toString()}`)
        .then(response => response.text())
        .then(data => {
            document.getElementById("chat-content").innerHTML = data;
        })
        .catch(error => {
            console.error("Error fetching chat:", error);
            document.getElementById("chat-content").innerHTML = `<p>حدث خطأ أثناء تحميل المحادثة.</p>`;
        });
}
function sendMessage() {
    var messageContent = document.getElementById('message').value;
    var receiverId = document.getElementById('ReceiverId').value;
    var messageType = document.getElementById('Type').value;

    // إرسال الطلب عبر AJAX
    $.ajax({
        type: "post",
        url: "/Chat/SendMessage/",  // الإرسال إلى الرابط المخصص
        data: {
            MessageContent: messageContent,
            ReceiverId: receiverId,
            Type: messageType
        },
        success: function (response) {
            console.log('Response received:', response);
            var message = response.message; // استلام الرسالة من الـ response
            var user = response.user;
            var sentAt = response.sentAt;

            var chatClass = message.senderId == user.userId ? "text-right" : "";
            var messageClass = message.senderId == user.userId ? "other-message float-right" : "my-message";
            var newMessage = '<li class="clearfix ' + chatClass + '">' +
                '<div class="message-data ' + message.type + '">' +
                '<span class="message-data-time">' + sentAt + '</span>';

            // عرض صورة المستخدم إذا كان هو المرسل
            if (message.senderId == user.userId) {
                newMessage += '<img src="' + user.profileImageFileName + '" alt="avatar">';
            }

            newMessage += '</div>' +
                '<div class="message ' + messageClass + '">' + message.messageContent + '</div>' +
                '</li>';
            console.log('Response received:', newMessage);
            // إضافة الرسالة الجديدة إلى عنصر <ul> في واجهة المحادثة
            $('.chat-history ul').append(newMessage);


            document.getElementById('message').value = ''; // إفراغ حقل الرسالة
        }
    });
}


const connection = new signalR.HubConnectionBuilder()
    .withUrl("/chatHub")
    .build();

connection.on("ReceiveMessage", (user, message, sentAt) => {
    if (currentChatId === user.userId) {
        const chatHistory = document.querySelector(".chat-history ul");
        const chatClass = message.senderId === user.userId ? "text-right" : "";
        const messageClass = message.senderId === user.userId ? "other-message float-right" : "my-message";

        const li = document.createElement("li");
        li.classList.add("clearfix");

        li.innerHTML = `
        <div class="message-data ${chatClass}">
            <span class="message-data-time">${sentAt}</span>
            ${message.senderId === user.userId ? `<img src=${user.profileImageFileName} alt = "avatar" >` : ""}
        </div>
        <div class="message ${messageClass}">${message.messageContent}</div>
    `;
        chatHistory.appendChild(li);
    }
});

connection.start().catch(err => console.error(err.toString()));
