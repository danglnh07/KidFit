// KidFit Learning Hub - Site JavaScript

// Initialize Quill editor
function initQuillEditor(elementId, content) {
    const quill = new Quill(elementId, {
        theme: 'snow',
        placeholder: 'Enter content here...',
        modules: {
            toolbar: [
                [{ 'header': [1, 2, 3, 4, 5, 6, false] }],
                ['bold', 'italic', 'underline', 'strike'],
                [{ 'color': [] }, { 'background': [] }],
                [{ 'script': 'sub'}, { 'script': 'super' }],
                [{ 'list': 'ordered'}, { 'list': 'bullet' }],
                [{ 'indent': '-1'}, { 'indent': '+1' }],
                [{ 'align': [] }],
                ['blockquote', 'code-block'],
                ['link', 'image'],
                ['clean']
            ]
        }
    });

    // Handle image uploads (convert to base64)
    quill.getModule('toolbar').addHandler('image', function() {
        const input = document.createElement('input');
        input.setAttribute('type', 'file');
        input.setAttribute('accept', 'image/*');
        input.click();

        input.onchange = () => {
            const file = input.files[0];
            if (file) {
                const reader = new FileReader();
                reader.onload = (e) => {
                    const range = quill.getSelection();
                    quill.insertEmbed(range.index, 'image', e.target.result);
                };
                reader.readAsDataURL(file);
            }
        };
    });

    // Set initial content if provided
    if (content) {
        // Convert markdown to HTML for Quill
        const htmlContent = marked.parse(content);
        quill.root.innerHTML = htmlContent;
    }

    return quill;
}

// Get markdown content from Quill editor
function getMarkdownFromQuill(quill) {
    const turndownService = new TurndownService({
        headingStyle: 'atx',
        bulletListMarker: '-',
        codeBlockStyle: 'fenced'
    });
    
    // Configure turndown to handle images properly
    turndownService.addRule('img', {
        filter: 'img',
        replacement: function(content, node) {
            const alt = node.alt || '';
            const src = node.getAttribute('src') || '';
            return `![${alt}](${src})`;
        }
    });
    
    const html = quill.root.innerHTML;
    return turndownService.turndown(html);
}

// Render markdown content
function renderMarkdown(elementId, markdownContent) {
    const element = document.getElementById(elementId);
    if (element && markdownContent) {
        element.innerHTML = marked.parse(markdownContent);
    }
}

// Initialize markdown renderers on page load
document.addEventListener('DOMContentLoaded', function() {
    // Find all elements with data-markdown attribute
    const markdownElements = document.querySelectorAll('[data-markdown]');
    markdownElements.forEach(el => {
        const content = el.getAttribute('data-markdown');
        if (content) {
            el.innerHTML = marked.parse(content);
        }
    });

    // Initialize tooltips
    const tooltipTriggerList = document.querySelectorAll('[data-bs-toggle="tooltip"]');
    if (tooltipTriggerList.length > 0) {
        tooltipTriggerList.forEach(el => new bootstrap.Tooltip(el));
    }

    // Auto-hide alerts after 5 seconds
    const alerts = document.querySelectorAll('.alert-dismissible');
    alerts.forEach(alert => {
        setTimeout(() => {
            const bsAlert = bootstrap.Alert.getOrCreateInstance(alert);
            if (bsAlert) {
                bsAlert.close();
            }
        }, 5000);
    });
});

// Preview uploaded image
function previewImage(input, previewId) {
    const preview = document.getElementById(previewId);
    if (input.files && input.files[0]) {
        const reader = new FileReader();
        reader.onload = function(e) {
            preview.src = e.target.result;
            preview.style.display = 'block';
        };
        reader.readAsDataURL(input.files[0]);
    }
}

// Confirm delete action
function confirmDelete(formId, itemName) {
    if (confirm(`Are you sure you want to delete "${itemName}"? This action cannot be undone.`)) {
        document.getElementById(formId).submit();
    }
}

// Format date for display
function formatDate(dateString) {
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', {
        year: 'numeric',
        month: 'short',
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
    });
}
