import { mande, type MandeError } from 'mande';

/**
 * Runs a PUT action against a Freistellung endpoint, showing a success/error toast and
 * returning whether it succeeded. Collapses the try/catch/toast boilerplate that every
 * mutating action in this module otherwise repeats.
 */
export function useFreistellungAction() {
    const toast = useToast();

    async function run(
        path: string,
        body: Record<string, unknown> | null,
        successToast?: { title: string; description: string },
    ): Promise<boolean> {
        const api = mande(path);
        try {
            await api.put(body ?? {});
            if (successToast) {
                toast.add({ color: 'success', ...successToast });
            }
            return true;
        } catch (e) {
            const mandeError = e as MandeError<{ error?: string }>;
            toast.add({
                color: 'error',
                title: 'Fehler',
                description:
                    mandeError.body?.error ?? 'Ein unbekannter Fehler ist aufgetreten.',
            });
            return false;
        }
    }

    return { run };
}
