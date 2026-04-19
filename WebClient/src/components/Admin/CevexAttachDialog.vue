<script lang="ts" setup>
import { inject, ref } from 'vue';
import type { CevexEntity } from '@/models/admin/cevex';
import type { UserInfoMinimal } from '@/models/user/userInfoMinimal';
import { formatStudent } from '@/helpers/formatters';
import { Button, FloatLabel, Select } from 'primevue';

const dialogRef = inject<{
    data: { options: CevexEntity[]; student: UserInfoMinimal };
    close: (data?: any) => void;
}>('dialogRef');
const selected = ref<CevexEntity | null>(null);
</script>

<template>
    <p>
        Bitte wählen Sie die Cevex-Schüler:in aus, die der Nutzer:in
        <strong class="inline-block">{{ formatStudent(dialogRef.data.student) }}</strong>
        entspricht.
    </p>

    <div class="flex flex-col gap-4">
        <FloatLabel variant="on">
            <Select
                id="cevex-user"
                v-model="selected"
                :option-label="(data) => `${data.firstName} ${data.lastName}`"
                :options="dialogRef.data.options"
                fluid
            />
            <label for="cevex-user">Cevex Schüler:in</label>
        </FloatLabel>

        <div class="flex flex-row gap-4">
            <Button label="Abbrechen" severity="secondary" @click="dialogRef.close()" />
            <Button
                :disabled="!selected"
                fluid
                label="Zuweisen"
                severity="primary"
                @click="dialogRef.close({ result: selected })"
            />
        </div>
    </div>
</template>

<style scoped></style>
