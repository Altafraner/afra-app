<script setup>
import { Button, InputGroup, Message, Tag, useDialog, useToast } from 'primevue';
import { ref } from 'vue';
import { formatDate, formatTutor } from '@/helpers/formatters.ts';
import { mande } from 'mande';
import { useUser } from '@/stores/user';
import { useOtiumStore } from '@/Otium/stores/otium.js';
import { useRouter } from 'vue-router';
import AfraKategorieTag from '@/Otium/components/Shared/AfraKategorieTag.vue';
import { findPath } from '@/helpers/tree.js';
import SimpleBreadcrumb from '@/components/SimpleBreadcrumb.vue';
import MultipleEnrollmentForm from '@/Otium/components/Katalog/Forms/MultipleEnrollmentForm.vue';
import { useConfirmPopover } from '@/composables/confirmPopover.js';
import Notes from '@/Otium/components/Notes/Notes.vue';

const settings = useOtiumStore();
const user = useUser();
const { openConfirmDialog } = useConfirmPopover();
const toast = useToast();
const router = useRouter();
const dialog = useDialog();
const props = defineProps({
    terminId: String,
});
const emit = defineEmits(['update']);

const buttonLoading = ref(true);
const otium = ref(null);
const connection = ref(null);

async function loadTermin() {
    buttonLoading.value = true;
    try {
        otium.value = await connection.value.get();
        buttonLoading.value = false;
    } catch (error) {
        toast.add({
            severity: 'error',
            summary: 'Fehler',
            detail: 'Es ist ein Fehler beim Laden aufgetreten.',
        });
        await router.push('/katalog');
        await user.update();
    }
}

async function unenroll() {
    buttonLoading.value = true;
    try {
        otium.value = await connection.value.delete();
        buttonLoading.value = false;
    } catch (error) {
        toast.add({
            severity: 'error',
            summary: 'Fehler',
            detail: 'Es ist ein Fehler beim Austragen aufgetreten.',
        });
    } finally {
        emit('update');
    }
}

async function enroll() {
    buttonLoading.value = true;
    try {
        otium.value = await connection.value.put();
        buttonLoading.value = false;
    } catch (error) {
        toast.add({
            severity: 'error',
            summary: 'Fehler',
            detail: 'Es ist ein Fehler beim Einschreiben aufgetreten.',
        });
    } finally {
        emit('update');
    }
}

function multiEnroll() {
    buttonLoading.value = true;
    dialog.open(MultipleEnrollmentForm, {
        props: {
            header: 'Mehrfach einschreiben',
            modal: true,
            class: 'sm:max-w-xl',
        },
        data: {
            options: otium.value.wiederholungen,
        },
        onClose: multiEnrollCallback,
    });

    async function multiEnrollCallback(options) {
        try {
            if (options.data === undefined || options.data === null) return;
            if (options.data.selected.length === 0) return enroll();
            const response = await mande('/api/otium/' + props.terminId + '/multi-enroll').put(
                options.data.selected,
            );
            if (response.denied.length > 0) {
                toast.add({
                    severity: 'warn',
                    summary: 'Einschreibung teilweise fehlgeschlagen',
                    detail: `Die Einschreibung in die folgenden Termine ist fehlgeschlagen: ${response.denied.map((d) => formatDate(new Date(d))).join(', ')}`,
                });
            }
        } catch (err) {
            if (err.response)
                toast.add({
                    severity: 'error',
                    summary: 'Fehler',
                    detail: `Es ist ein Fehler beim Einschreiben aufgetreten. Code: ${err.response.status} (${err.response.statusText})`,
                });
            else {
                toast.add({
                    severity: 'error',
                    summary: 'Fehler',
                    detail: 'Es ist ein Fehler beim Einschreiben aufgetreten.',
                });
                console.error(err);
            }
        } finally {
            await loadTermin();
            buttonLoading.value = false;
        }
    }
}

async function edit(termin) {
    await router.push({
        name: 'Verwaltung-Termin',
        params: { terminId: termin.id },
    });
}

async function cancel(evt, termin) {
    const callback = async () => {
        const api = mande(`/api/otium/management/termin/${termin.id}/cancel`);
        try {
            await api.put();
            emit('update');
        } catch {
            toast.add({
                severity: 'error',
                summary: 'Fehler',
                detail: 'Der Termin konnte nicht abgesagt werden.',
            });
        }
    };
    openConfirmDialog(evt, callback, 'Termin absagen?');
}

async function editNotes() {
    dialog.open(Notes, {
        props: {
            modal: true,
            header: 'Notizen',
        },
        data: {
            notes: otium.value.einschreibung.notizen,
            myNote: otium.value.einschreibung.notiz,
            blockId: otium.value.block.id,
        },
        emits: {
            onUpdate: () => {
                loadTermin();
            },
        },
    });
}

async function setup() {
    connection.value = mande('/api/otium/' + props.terminId);
    await loadTermin();
}

await setup();
</script>

<template>
    <div class="grid auto-rows-[1fr] grid-cols-[1fr_auto] items-center">
        <!-- Row 1 Column 1 -->
        <div class="flex flex-row gap-4 flex-wrap min-h-8">
            <Tag v-if="otium.istAbgesagt" icon="pi pi-exclamation-triangle" severity="danger"
                >Abgesagt
            </Tag>
            <span v-if="otium.tutor">
                <i class="pi pi-user" />
                {{ formatTutor(otium.tutor) }}
            </span>
            <span v-if="otium.ort"> <i class="pi pi-map-marker" /> {{ otium.ort }} </span>
            <span v-if="otium.block.datum">
                <i class="pi pi-clock" />
                {{ formatDate(new Date(otium.block.datum)) }},
                {{ otium.block.uhrzeit.start }} Uhr
            </span>
        </div>

        <!-- Row 1 Column 2 -->
        <template v-if="user.isStudent">
            <Button
                v-if="otium.istAbgesagt"
                disabled
                icon="pi pi-exclamation-triangle"
                label="Abgesagt"
                severity="danger"
                variant="text"
            />
            <div
                v-else-if="otium.einschreibung.eingeschrieben"
                class="flex flex-col gap-3 items-end"
            >
                <Button
                    v-if="otium.einschreibung.kannBearbeiten"
                    :disabled="buttonLoading"
                    :loading="buttonLoading"
                    icon="pi pi-times"
                    label="Austragen"
                    severity="danger"
                    variant="text"
                    @click="() => unenroll()"
                />
                <Button
                    v-else
                    v-tooltip.left="otium.einschreibung.grund"
                    disabled
                    icon="pi pi-times"
                    label="Austragen"
                    severity="danger"
                    variant="text"
                />
            </div>
            <template v-else>
                <Button
                    v-if="otium.einschreibung.kannBearbeiten"
                    :disabled="buttonLoading"
                    :loading="buttonLoading"
                    class="justify-end"
                    fluid
                    icon="pi pi-plus"
                    label="Einschreiben"
                    variant="text"
                    @click="() => enroll()"
                />
                <Button
                    v-else
                    v-tooltip.left="otium.einschreibung.grund"
                    :loading="buttonLoading"
                    disabled
                    icon="pi pi-plus"
                    label="Einschreiben"
                    variant="text"
                />
            </template>
        </template>
        <span v-else>
            <!-- At some point we'll add functionality to force enroll a student here -->
        </span>
        <!-- Row 2 Column 1 -->
        <SimpleBreadcrumb :model="findPath(settings.kategorien, otium.kategorie)" wrap>
            <template #item="{ item }">
                <AfraKategorieTag :value="item" minimal />
            </template>
        </SimpleBreadcrumb>

        <!-- Row 2 Column 2 -->
        <template v-if="user.isStudent">
            <Button
                v-if="otium.einschreibung.eingeschrieben"
                :loading="buttonLoading"
                :disabled="buttonLoading"
                icon="pi pi-clipboard"
                :severity="
                    otium.einschreibung.notiz !== null ||
                    otium.einschreibung.notizen.length !== 0
                        ? 'warn'
                        : 'secondary'
                "
                label="Notizen"
                variant="text"
                @click="editNotes"
            />
            <Button
                v-else-if="
                    !otium.einschreibung.eingeschrieben &&
                    otium.einschreibung.kannBearbeiten &&
                    otium.wiederholungen.length > 0
                "
                :disabled="buttonLoading"
                :loading="buttonLoading"
                icon="pi pi-refresh"
                label="Mehrmals Einschreiben"
                severity="secondary"
                variant="text"
                @click="() => multiEnroll()"
            />
        </template>
        <template v-else-if="user.isOtiumsverantwortlich">
            <InputGroup>
                <Button
                    v-tooltip.top="'Bearbeiten'"
                    aria-label="Bearbeiten"
                    icon="pi pi-pencil"
                    severity="secondary"
                    variant="text"
                    @click="() => edit(otium)"
                />
                <Button
                    v-tooltip.top="'Absagen'"
                    aria-label="Absagen"
                    icon="pi pi-stop"
                    severity="danger"
                    variant="text"
                    @click="(evt) => cancel(evt, otium)"
                />
            </InputGroup>
        </template>
        <span v-else />
    </div>

    <h3 class="font-bold mt-4 text-lg">Beschreibung</h3>
    <p
        v-for="beschreibung in otium.beschreibung.split('\n').filter((desc) => desc)"
        v-if="!props.minimal && otium.beschreibung"
    >
        {{ beschreibung }}
    </p>

    <Message v-if="user.isStudent && otium.einschreibung.grund" class="mt-4" severity="warn"
        >{{ otium.einschreibung.grund }}
    </Message>
</template>

<style scoped></style>
