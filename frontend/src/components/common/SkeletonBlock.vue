<script setup lang="ts">
withDefaults(
  defineProps<{
    lines?: number;
  }>(),
  {
    lines: 3
  }
);
</script>

<template>
  <div class="skeleton-block" aria-hidden="true">
    <span v-for="line in lines" :key="line" class="skeleton" :style="{ width: `${100 - (line - 1) * 14}%` }" />
  </div>
</template>

<style scoped>
.skeleton-block {
  display: grid;
  gap: 10px;
}

.skeleton-block span {
  display: block;
  height: 14px;
}
</style>
