function selectChat(element) {
    const chatId = element.getAttribute("data-id");

    // تحديث واجهة المستخدم لتظهر أن المستخدم اختار محادثة
    document.getElementById("chat-content").innerHTML = `<p>جارٍ تحميل محادثة المستخدم ${chatId}...</p>`;

    // إرسال طلب إلى السيرفر لجلب بيانات المحادثة
    fetch(`/Chat/GetChat?chatId=${chatId}`)
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
