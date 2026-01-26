import { marked } from 'marked';
import DOMPurify from 'dompurify';

export function convertMarkdownToHtml(markdown: string, inline: boolean = false): string {
    const markdownAsHtml = inline
        ? marked.parseInline(markdown, { async: false })
        : marked.parse(markdown, { async: false });
    return DOMPurify.sanitize(markdownAsHtml);
}
