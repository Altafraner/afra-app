<script setup>
import {ref} from "vue";
import AfraOtium from "@/components/Otium/AfraOtium.vue";
import AfraOtiumInstance from "@/components/Otium/AfraOtiumInstance.vue";
import {otium as testOtium, terminEinschreibungsDetails} from "@/helpers/testdata.js";

const otium = ref(testOtium)

const instance = ref(terminEinschreibungsDetails)
</script>

<template>
  <div class="flex justify-center">
    <div class="this_container">
      <h1>Otium</h1>
      <afra-otium :otium="otium" may-enroll may-edit/>
      <afra-otium-instance :otium="instance" />
    </div>
  </div>
</template>

<style scoped>
  .this_container{
    width: 50rem;
  }
</style>
