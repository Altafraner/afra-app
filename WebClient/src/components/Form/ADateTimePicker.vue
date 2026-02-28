<script lang="ts" setup>
import { CalendarDateTime } from '@internationalized/date';

const modelValue = defineModel<CalendarDateTime | undefined>();
</script>

<template>
    <UPopover>
        <UInputDate ref="inputDate" v-model="modelValue" granularity="minute">
            <template #trailing>
                <UButton
                    aria-label="Datum und Uhrzeit auswählen"
                    class="px-0"
                    color="neutral"
                    icon="i-lucide-calendar"
                    size="sm"
                    variant="link"
                />
            </template>
        </UInputDate>

        <template #content>
            <UCalendar v-model="modelValue" class="p-2" granularity="minute" />
        </template>
    </UPopover>
</template>
