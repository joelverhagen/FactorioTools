<template>
  <div class="card mt-3 rounded-0">
    <h4 class="card-header bg-danger bg-opacity-10">
      {{ error.title }}
    </h4>
    <ul class="list-group list-group-flush" v-for="[key, values] in Object.entries(error.errors)" v-if="error.errors">
      <template v-for="value in values">
        <li class="list-group-item">{{ value }}</li>
      </template>
    </ul>
    <div class="card-body" v-for="value in error.errorDetails" v-if="error.errorDetails">
      <pre class="mb-0">{{ value }}</pre>
    </div>
    <div class="card-body" v-if="!error.errors && error.response?.error">
      <pre class="mb-0">{{ JSON.stringify(error.response.error, null, '  ') }}</pre>
    </div>
  </div>
</template>
      
<script lang="ts">
import { PropType } from 'vue';
import { ResponseError } from '../lib/OilFieldPlanner';

export default {
  props: {
    error: {
      type: Object as PropType<ResponseError>,
      required: true
    }
  }
}
</script>