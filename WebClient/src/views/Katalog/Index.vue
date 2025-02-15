<script setup>
import {OtiumsKategorie} from "@/constants/OtiumsKategorie.js";
import AfraOtiumKategorieButton from "@/components/AfraOtiumKategorieButton.vue";

console.log(OtiumsKategorie)
</script>

<template>
  <h1>Otia-Katalog</h1>
  <p>Bitte wähle ein Gebiet aus!</p>
  <div class="flex flex-col gap-5">
    <afra-otium-kategorie-button :icon="OtiumsKategorie['akademisches'].icon"
                                 :color="OtiumsKategorie['akademisches'].css_color"
                                 :label="OtiumsKategorie['akademisches'].label"
                                 to="/katalog/akademisches"/>
    <div class="ot-category-container gap-5">
      <afra-otium-kategorie-button
        v-for="name of Object.keys(OtiumsKategorie).filter(o => o!=='akademisches')" key="name"
        :label="OtiumsKategorie[name].label" :icon="OtiumsKategorie[name].icon"
        :color="OtiumsKategorie[name].css_color" :to="`/katalog/${name}`"/>
    </div>
  </div>
</template>

<style scoped>
.ot-category-container {
  display: grid;
  grid-template-columns: 1fr 1fr;
  grid-template-rows: 1fr 1fr;
}


</style>
