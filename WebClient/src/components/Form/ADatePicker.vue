<script lang="ts" setup>
import { CalendarDate } from '@internationalized/date';

const modelValue = defineModel<CalendarDate | undefined>();
</script>

<template>
    <UPopover>
        <UInputDate ref="inputDate" v-model="modelValue">
            <template #trailing>
                <UButton
                    aria-label="Select a date"
                    class="px-0"
                    color="neutral"
                    icon="i-lucide-calendar"
                    size="sm"
                    variant="link"
                />
            </template>
        </UInputDate>

        <template #content>
            <UCalendar v-model="modelValue" class="p-2" />
        </template>
    </UPopover>
</template>
