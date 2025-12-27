import { marked } from 'marked';
import DOMPurify from 'dompurify';

export function convertMarkdownToHtml(markdown: string): string {
    const markdownAsHtml = marked.parse(markdown, { async: false });
    return DOMPurify.sanitize(markdownAsHtml);
}
