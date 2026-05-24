<script setup lang="ts">
import PageHeader from './PageHeader.vue';

defineProps<{
  title: string;
  description: string;
}>();
</script>

<template>
  <section class="page">
    <PageHeader :title="title" :description="description">
      <template #actions>
        <slot name="actions" />
      </template>
    </PageHeader>
    <slot />
  </section>
</template>
