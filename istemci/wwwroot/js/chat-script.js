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
    console.log('groupId:', document.getElementById('groupId').value);
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

            var chatClass = message.senderId != myId ? "text-right" : "";
            var messageClass = message.senderId != myId ? "other-message float-right" : "my-message";
            var newMessage = '<li class="clearfix ' + chatClass + '">' +
                '<div class="message-data ' + message.type + '">' +
                '<span class="message-data-time">' + sentAt + '</span>';

            // عرض صورة المستخدم إذا كان هو المرسل
            //if (message.senderId != myId) {
            //    newMessage += '<img src="' + message.profileImageFileName + '" alt="avatar">';
            //}

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

function JoinGroup() {
    var groupId = document.getElementById('GroupId').value;

    $.ajax({
        type: "post",
        url: "/Chat/GroupId/", 
        data: {
            GroupId: groupId
        },
        success: function (response) {
            var GroupId = response.groupId; 
            var GroupImageUrl = response.groupImageUrl; 
            var GroupName = response.groupName;
            console.log('GroupId', GroupId);
            console.log('GroupImageUrl', GroupImageUrl);
            console.log('GroupName', GroupName);

            
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

connection.on("ReceiveMessage", (user, message, sentAt, myId) => {
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
        <div class="message-data ${chatClass}">
            <span class="message-data-time">${sentAt}</span>
            ${message.senderId !== myId
                ? `<img src="${user.profileImageFileName}" alt="avatar">
                       <span class="message-data-name">${user.username}</span>`
                : ""
            }
        </div>
        <div class="message ${messageClass}">${message.messageContent}</div>
        `;
        console.log('li:', li);

        chatHistory.appendChild(li);

        // تحريك شريط التمرير لأسفل عند إضافة رسالة جديدة
        chatHistory.scrollTop = chatHistory.scrollHeight;
    }
});


connection.start().catch(err => console.error(err.toString()));
