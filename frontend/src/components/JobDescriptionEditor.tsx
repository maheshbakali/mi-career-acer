import { EditorContent, useEditor } from "@tiptap/react";
import StarterKit from "@tiptap/starter-kit";

type Props = {
  value: string;
  onChange: (html: string) => void;
};

export function JobDescriptionEditor({ value, onChange }: Props) {
  const editor = useEditor({
    extensions: [StarterKit],
    content: value && value !== "" ? value : "<p></p>",
    editorProps: {
      attributes: {
        class:
          "prose prose-invert max-w-none min-h-[200px] rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm focus:outline-none",
      },
    },
    onUpdate: ({ editor }) => onChange(editor.getHTML()),
  });

  if (!editor) return <div className="h-[200px] animate-pulse rounded-lg bg-slate-900" />;

  return (
    <div className="space-y-2">
      <div className="flex flex-wrap gap-2">
        <button
          type="button"
          className="rounded border border-slate-700 px-2 py-1 text-xs hover:bg-slate-900"
          onClick={() => editor.chain().focus().toggleBold().run()}
        >
          Bold
        </button>
        <button
          type="button"
          className="rounded border border-slate-700 px-2 py-1 text-xs hover:bg-slate-900"
          onClick={() => editor.chain().focus().toggleItalic().run()}
        >
          Italic
        </button>
        <button
          type="button"
          className="rounded border border-slate-700 px-2 py-1 text-xs hover:bg-slate-900"
          onClick={() => editor.chain().focus().toggleBulletList().run()}
        >
          List
        </button>
      </div>
      <EditorContent editor={editor} />
    </div>
  );
}
