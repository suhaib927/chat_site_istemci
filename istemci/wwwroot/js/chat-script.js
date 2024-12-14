function selectChat(element) {
    const chatId = element;

    // إرسال طلب إلى السيرفر لجلب بيانات المحادثة
    fetch(`/Chat/LoadChat?chatId=${chatId}`)
        .then(response => response.text())
        .then(data => {
            document.getElementById("chat-content").innerHTML = data;
        })
        .catch(error => {
            console.error("Error fetching chat:", error);
            document.getElementById("chat-content").innerHTML = `<p>حدث خطأ أثناء تحميل المحادثة.</p>`;
        });
}
