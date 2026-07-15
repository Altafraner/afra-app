<script setup>
import { computed, ref } from 'vue';
import { formatDate, formatTutor } from '@/helpers/formatters';
import { mande } from 'mande';
import { useUser } from '@/stores/user';
import { useOtiumStore } from '@/Otium/stores/otium.js';
import { useRouter } from 'vue-router';
import OtiumKategorieTag from '@/Otium/components/Shared/OtiumKategorieTag.vue';
import { findPath } from '@/helpers/tree.js';
import SimpleBreadcrumb from '@/components/SimpleBreadcrumb.vue';
import MultipleEnrollmentForm from '@/Otium/components/Katalog/Forms/MultipleEnrollmentForm.vue';
import { useConfirmPopover } from '@/composables/confirmPopover';
import Notes from '@/Attendance/components/Notes.vue';
import { convertMarkdownToHtml } from '@/composables/markdown.ts';
import MobileSwitch from '@/components/MobileSwitch.vue';

const settings = useOtiumStore();
const user = useUser();
const { requireConfirm } = useConfirmPopover();
const toast = useToast();
const router = useRouter();
const overlay = useOverlay();
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
            color: 'error',
            title: 'Fehler',
            description: 'Es ist ein Fehler beim Laden aufgetreten.',
        });
        await router.push({ name: 'Otium-Katalog' });
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
            color: 'error',
            title: 'Fehler',
            description: 'Es ist ein Fehler beim Austragen aufgetreten.',
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
            color: 'error',
            title: 'Fehler',
            description: 'Es ist ein Fehler beim Einschreiben aufgetreten.',
        });
    } finally {
        emit('update');
    }
}

async function multiEnroll() {
    buttonLoading.value = true;
    const modal = overlay.create(MultipleEnrollmentForm);
    const options = await modal.open({ options: otium.value.wiederholungen });

    if (!options) {
        buttonLoading.value = false;
        return;
    }
    try {
        if (options === 0) {
            await enroll();
            return;
        }
        const response = await mande('/api/otium/' + props.terminId + '/multi-enroll').put(
            options,
        );
        if (response.denied.length > 0) {
            toast.add({
                color: 'warn',
                title: 'Einschreibung teilweise fehlgeschlagen',
                description: `Die Einschreibung in die folgenden Termine ist fehlgeschlagen: ${response.denied.map((d) => formatDate(new Date(d))).join(', ')}`,
            });
        }
    } catch (err) {
        if (err.response)
            toast.add({
                color: 'error',
                title: 'Fehler',
                description: `Es ist ein Fehler beim Einschreiben aufgetreten. Code: ${err.response.status} (${err.response.statusText})`,
            });
        else {
            toast.add({
                color: 'error',
                title: 'Fehler',
                description: 'Es ist ein Fehler beim Einschreiben aufgetreten.',
            });
            console.error(err);
        }
    } finally {
        await loadTermin();
        buttonLoading.value = false;
        emit('update');
    }
}

async function edit(termin) {
    await router.push({
        name: 'Verwaltung-Termin',
        params: { terminId: termin.id },
    });
}

async function cancel(termin) {
    const confirmed = await requireConfirm('Wollen Sie den Termin wirklich absagen?');
    if (!confirmed) return;
    const api = mande(`/api/otium/management/termin/${termin.id}/cancel`);
    try {
        await api.put();
        emit('update');
    } catch {
        toast.add({
            color: 'error',
            title: 'Fehler',
            description: 'Der Termin konnte nicht abgesagt werden.',
        });
    }
}

async function editNotes() {
    const modal = overlay.create(Notes);
    await modal.open({
        notes: otium.value.einschreibung.notizen,
        myNote: otium.value.einschreibung.notiz,
        slotId: otium.value.block.id,
        scope: 'otium',
        studentId: user.user.id,
        updateSelf: true,
    });
    await loadTermin();
}

async function setup() {
    connection.value = mande('/api/otium/' + props.terminId);
    await loadTermin();
}

await setup();

const description = computed(() => {
    return otium.value ? convertMarkdownToHtml(otium.value.beschreibung) : null;
});
</script>

<template>
    <MobileSwitch>
        <template #large>
            <div class="grid auto-rows-[1fr] grid-cols-[1fr_auto] items-center gap-1">
                <!-- Row 1 Column 1 -->
                <div class="flex flex-row gap-4 flex-wrap min-h-8">
                    <UBadge
                        v-if="otium.istAbgesagt"
                        color="error"
                        icon="i-lucide-triangle-alert"
                        >Abgesagt
                    </UBadge>
                    <span v-if="otium.tutor" class="inline-flex items-center gap-1">
                        <UIcon class="size-4" name="i-lucide-user" />
                        {{ formatTutor(otium.tutor) }}
                    </span>
                    <span v-if="otium.ort" class="inline-flex items-center gap-1">
                        <UIcon class="size-4" name="i-lucide-map-pin" /> {{ otium.ort }}
                    </span>
                    <span v-if="otium.block.datum" class="inline-flex items-center gap-1">
                        <UIcon class="size-4" name="i-lucide-clock" />
                        {{ formatDate(new Date(otium.block.datum)) }},
                        {{ otium.block.uhrzeit.start }} Uhr
                    </span>
                    <span class="inline-flex items-center gap-1">
                        <UIcon class="size-4" name="i-lucide-users" />
                        {{ otium.maxEinschreibungen ?? '—' }}
                    </span>
                </div>

                <!-- Row 1 Column 2 -->
                <template v-if="user.isStudent">
                    <UButton
                        v-if="otium.istAbgesagt"
                        disabled
                        color="error"
                        label="Abgesagt"
                        icon="i-lucide-triangle-alert"
                        variant="subtle"
                    />
                    <div
                        v-else-if="otium.einschreibung.eingeschrieben"
                        class="flex flex-col gap-3 items-end"
                    >
                        <UButton
                            :disabled="!otium.einschreibung.kannBearbeiten"
                            :loading="buttonLoading"
                            class="w-full"
                            color="error"
                            label="Austragen"
                            icon="i-lucide-x"
                            size="lg"
                            variant="subtle"
                            @click="() => unenroll()"
                        />
                    </div>
                    <template v-else>
                        <UButton
                            :disabled="!otium.einschreibung.kannBearbeiten"
                            :loading="buttonLoading"
                            class="w-full"
                            icon="i-lucide-plus"
                            label="Einschreiben"
                            size="lg"
                            variant="subtle"
                            @click="() => enroll()"
                        />
                    </template>
                </template>
                <span v-else>
                    <!-- At some point we'll add functionality to force enroll a student here -->
                </span>
                <!-- Row 2 Column 1 -->
                <SimpleBreadcrumb :model="findPath(settings.kategorien, otium.kategorie)" wrap>
                    <template #item="{ item }">
                        <OtiumKategorieTag :value="item" minimal />
                    </template>
                </SimpleBreadcrumb>

                <!-- Row 2 Column 2 -->
                <template v-if="user.isStudent">
                    <UButton
                        v-if="otium.einschreibung.eingeschrieben"
                        :loading="buttonLoading"
                        :color="
                            otium.einschreibung.notiz !== null ||
                            otium.einschreibung.notizen.length !== 0
                                ? 'warning'
                                : 'secondary'
                        "
                        class="w-full"
                        label="Notizen"
                        icon="i-lucide-clipboard"
                        size="lg"
                        variant="subtle"
                        @click="editNotes"
                    />
                    <UButton
                        v-else-if="
                            !otium.einschreibung.eingeschrieben &&
                            otium.einschreibung.kannBearbeiten &&
                            otium.wiederholungen.length > 0
                        "
                        :loading="buttonLoading"
                        class="w-full"
                        label="Mehrmals Einschreiben"
                        color="secondary"
                        icon="i-lucide-refresh-cw"
                        size="lg"
                        variant="subtle"
                        @click="() => multiEnroll()"
                    />
                </template>
                <template v-else-if="user.isOtiumsverantwortlich">
                    <UFieldGroup>
                        <UTooltip text="Bearbeiten">
                            <UButton
                                aria-label="Bearbeiten"
                                color="secondary"
                                icon="i-lucide-pencil"
                                size="lg"
                                variant="ghost"
                                @click="() => edit(otium)"
                            />
                        </UTooltip>
                        <UTooltip text="Absagen">
                            <UButton
                                aria-label="Absagen"
                                color="error"
                                icon="i-lucide-square"
                                size="lg"
                                variant="ghost"
                                @click="() => cancel(otium)"
                            />
                        </UTooltip>
                    </UFieldGroup>
                </template>
                <span v-else />
            </div>

            <h3 class="font-bold mt-4 text-lg">Beschreibung</h3>
            <div v-if="!props.minimal && description" v-html="description" />

            <UAlert
                v-if="user.isStudent && otium.einschreibung.grund"
                class="mt-4"
                :description="otium.einschreibung.grund"
                color="warning"
                variant="subtle"
            />
        </template>
        <template #small>
            <div class="text-muted text-sm">
                <SimpleBreadcrumb
                    :model="findPath(settings.kategorien, otium.kategorie)"
                    class="inline-flex"
                    wrap
                >
                    <template #item="{ item }">
                        <OtiumKategorieTag :value="item" hideIcon minimal />
                    </template>
                </SimpleBreadcrumb>
                <template v-if="otium.maxEinschreibungen">
                    &CenterDot; Max. {{ otium.maxEinschreibungen }} Personen
                </template>
            </div>
            <div
                v-if="!props.minimal && description"
                class="mt-2 mb-3 text-justify hyphens-auto"
                v-html="description"
            />
            <UAlert
                v-if="user.isStudent && otium.einschreibung.grund"
                class="my-2"
                :description="otium.einschreibung.grund"
                color="warning"
                variant="subtle"
            />
            <div class="flex flex-col gap-2 my-4">
                <template v-if="user.isOtiumsverantwortlich">
                    <UButton
                        color="primary"
                        label="Bearbeiten"
                        icon="i-lucide-pencil"
                        @click="() => edit(otium)"
                    />
                    <UButton
                        color="error"
                        label="Absagen"
                        icon="i-lucide-square"
                        @click="() => cancel(otium)"
                    />
                </template>
                <template v-if="user.isStudent">
                    <UButton
                        v-if="otium.istAbgesagt"
                        disabled
                        color="error"
                        label="Abgesagt"
                        icon="i-lucide-triangle-alert"
                        size="lg"
                    />
                    <template v-else-if="otium.einschreibung.eingeschrieben">
                        <UButton
                            v-if="otium.einschreibung.kannBearbeiten"
                            :loading="buttonLoading"
                            color="error"
                            label="Austragen"
                            icon="i-lucide-x"
                            size="lg"
                            @click="() => unenroll()"
                        />
                        <UButton
                            v-else
                            disabled
                            color="error"
                            label="Austragen nicht möglich"
                            icon="i-lucide-x"
                            size="lg"
                        />
                    </template>
                    <UButton
                        v-else-if="otium.einschreibung.kannBearbeiten"
                        :loading="buttonLoading"
                        icon="i-lucide-plus"
                        label="Einschreiben"
                        size="lg"
                        @click="() => enroll()"
                    />
                    <UButton
                        v-if="
                            !otium.einschreibung.eingeschrieben &&
                            otium.einschreibung.kannBearbeiten &&
                            otium.wiederholungen.length > 0
                        "
                        :loading="buttonLoading"
                        color="secondary"
                        label="Mehrmals Einschreiben"
                        icon="i-lucide-refresh-cw"
                        size="lg"
                        @click="() => multiEnroll()"
                    />
                    <UButton
                        v-if="otium.einschreibung.eingeschrieben"
                        :loading="buttonLoading"
                        :color="
                            otium.einschreibung.notiz !== null ||
                            otium.einschreibung.notizen.length !== 0
                                ? 'warning'
                                : 'primary'
                        "
                        icon="i-lucide-clipboard"
                        label="Notizen"
                        size="lg"
                        @click="editNotes"
                    />
                </template>
            </div>
        </template>
    </MobileSwitch>
</template>

<style scoped></style>
