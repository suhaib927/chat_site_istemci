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
    var groupId = document.getElementById('groupId').value;
    var myId = document.getElementById('MyId').value;
    
    // إرسال الطلب عبر AJAX
    $.ajax({
        type: "post",
        url: "/Chat/SendMessage/",  // الإرسال إلى الرابط المخصص
        data: {
            MessageContent: messageContent,
            ReceiverId: receiverId,
            Type: messageType,
            GroupId: groupId,
            MyId: myId
        },
        success: function (response) {
            console.log('Response received:', response);
            var message = response.message; // استلام الرسالة من الـ response
            var user = response.user;
            var sentAt = response.sentAt;
            var group = response.group;
            var myId = response.myId;

            const chatClass = message.senderId !== myId ? "text-right" : "";
            const messageClass = message.senderId !== myId ? "other-message float-right" : "my-message";
            const chatHistory = document.querySelector(".chat-history ul");

            const li = document.createElement("li");
            li.classList.add("clearfix");

            li.innerHTML = `
<div class="message-container ${chatClass}">
   
    <div class="message-content ${messageClass}">
        <div class="message-header">
            <span class="message-data-name">${message.sender.username}</span>
        </div>

        <div class="message-text">
            ${message.messageContent}
        </div>

        <div class="message-time">
            ${sentAt}
        </div>
    </div>
</div>
`;

            chatHistory.appendChild(li);


            document.getElementById('message').value = ''; // إفراغ حقل الرسالة
        }
    });
}

function JoinGroup() {
    var groupId = document.getElementById('GroupId').value;

    $.ajax({
        type: "post",
        url: "/Group/GroupId/", 
        data: {
            GroupId: groupId
        },
        success: function (response) {
            var GroupId = response.groupId; 
            var GroupImageUrl = response.groupImageUrl; 
            var GroupName = response.groupName;

            
            var newGroup = `
        <li class="clearfix" onclick="selectChat('${GroupId}')">
            <img src="${GroupImageUrl}" alt="avatar">
            <div class="about">
                <div class="name">${GroupName}</div>
                <div class="status">
                    <span>Group Chat</span>
                </div>
            </div>
        </li>
    `;
            console.log('newGroup', newGroup);
            $('.groups-history ul').append(newGroup);

            document.getElementById('GroupId').value = '';
        },
        error: function (xhr, status, error) {
            console.error('Error joining group:', error, xhr.responseText);
            alert('An error occurred while joining the group.');
        }

    });
}



const connection = new signalR.HubConnectionBuilder()
    .withUrl("/chatHub")
    .build();

connection.on("ReceiveMessage", (message, sentAt, myId) => {
    console.log('message', message);
    let chat = null;
    if (message.type === "Private") {
        chat = message.senderId;
    } else {
        chat = message.groupId;
    }

    if (currentChatId === chat) {

        const chatClass = message.senderId !== myId ? "text-right" : "";
        const messageClass = message.senderId !== myId ? "other-message float-right" : "my-message";
        const chatHistory = document.querySelector(".chat-history ul");

        const li = document.createElement("li");
        li.classList.add("clearfix");

        li.innerHTML = `
<div class="message-container ${chatClass}">
    ${message.senderId !== myId
                ? `<img src="${message.sender.profileImageFileName}" alt="avatar" class="message-avatar">`
                : ""
            }
    <div class="message-content ${messageClass}">
        <div class="message-header">
            <span class="message-data-name">${message.sender.username}</span>
        </div>

        <div class="message-text">
            ${message.messageContent}
        </div>

        <div class="message-time">
            ${sentAt}
        </div>
    </div>
</div>
`;

        chatHistory.appendChild(li);

        chatHistory.scrollTop = chatHistory.scrollHeight;
    }
});

connection.start().catch(err => console.error(err.toString()));
