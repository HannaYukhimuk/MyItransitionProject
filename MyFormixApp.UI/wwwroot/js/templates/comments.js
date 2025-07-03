class CommentsManager {
    constructor() {
        this.initEvents();
    }

    initEvents() {
        $(document).on('click', '.reply-btn', (e) => this.handleReplyClick(e));
        $(document).on('submit', '.reply-form', (e) => this.handleReplySubmit(e));
        $(document).on('click', '.edit-btn', (e) => this.handleEditClick(e));
        $(document).on('click', '.cancel-edit', (e) => this.handleCancelEdit(e));
        $(document).on('submit', '.edit-form', (e) => this.handleEditSubmit(e));
    }

    handleReplyClick(e) {
        const commentId = $(e.currentTarget).data('comment-id');
        const replyForm = $(`.reply-form[data-comment-id="${commentId}"]`);
        replyForm.toggle();

        if (replyForm.is(":visible")) {
            replyForm.get(0).scrollIntoView({ behavior: 'smooth' });
        }
    }

    handleReplySubmit(e) {
    e.preventDefault();
    const form = $(e.currentTarget);
    const token = form.find('input[name="__RequestVerificationToken"]').val();
    const data = form.serialize();

    $.ajax({
        url: form.attr('action'), // Use the form's action attribute
        type: 'POST',
        data: data,
        headers: {
            'RequestVerificationToken': token
        },
        success: () => location.reload(),
        error: (xhr) => alert('Error submitting reply: ' + xhr.responseText)
    });
}

    handleEditClick(e) {
        const id = $(e.currentTarget).data('comment-id');
        const container = $(`.card[data-comment-id="${id}"]`);
        const originalText = container.find('.comment-text').text().trim();

        container.find('textarea[name="Text"]').val(originalText);
        container.find('.comment-text').hide();
        container.find('.edit-form').show();
    }

    handleCancelEdit(e) {
        const form = $(e.currentTarget).closest('.edit-form');
        const id = form.data('comment-id');
        const container = $(`.card[data-comment-id="${id}"]`);
        form.hide();
        container.find('.comment-text').show();
    }

    handleEditSubmit(e) {
        e.preventDefault();
        const form = $(e.currentTarget);
        const id = form.data('comment-id');
        const templateId = form.find('input[name="TemplateId"]').val();
        const token = form.find('input[name="__RequestVerificationToken"]').val();
        const data = form.serialize();

        $.ajax({
            url: `/templates/${templateId}/comments/${id}/edit`,
            method: 'POST',
            data: data,
            headers: {
                'RequestVerificationToken': token
            },
            success: () => location.reload(),
            error: (xhr) => alert('Ошибка при редактировании: ' + xhr.responseText)
        });
    }
}

$(document).ready(() => {
    new CommentsManager();
});
