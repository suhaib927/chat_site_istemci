function selectChat(element) {
    const chatId = element;

    fetch(`/Chat/LoadChat?chatId=${chatId}`)
        .then(response => response.text())
        .then(data => {
            // تحديث واجهة المستخدم بمحتوى المحادثة
            document.getElementById("chat-content").innerHTML = data;
        })
        .catch(error => {
            console.error("Error fetching chat:", error);
            document.getElementById("chat-content").innerHTML = `<p>حدث خطأ أثناء تحميل المحادثة.</p>`;
        });
}
