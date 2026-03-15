<script lang="ts" setup>
import { Skeleton } from 'primevue';
</script>

<template>
    <Skeleton height="4rem" width="100%" />
    <main class="flex justify-center min-h-[90vh] mt-4">
        <div class="container">
            <h1>
                <Skeleton height="3rem" width="60%" />
            </h1>
            <p class="flex gap-2 flex-col">
                <Skeleton height="1rem" width="100%" />
                <Skeleton height="1rem" width="100%" />
                <Skeleton height="1rem" width="60%" />
            </p>
            <p class="flex gap-2 flex-col">
                <Skeleton height="1rem" width="100%" />
                <Skeleton height="1rem" width="100%" />
                <Skeleton height="1rem" width="100%" />
                <Skeleton height="1rem" width="30%" />
            </p>
        </div>
    </main>
</template>

<style scoped>
.container {
    width: 50rem;
}

@media screen and (width < 55rem) {
    .container {
        width: 95%;
    }
}
</style>
