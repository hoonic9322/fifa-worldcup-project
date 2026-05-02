import { createRouter, createWebHistory } from 'vue-router'

import Home from '../pages/Home.vue'
import Login from '../pages/Login.vue'
import Question from '../pages/Question.vue'

const routes = [
  {
    path: '/',
    name: 'Home',
    component: Home,
  },
  {
    path: '/login',
    name: 'Login',
    component: Login,
  },
  {
    path: '/question',
    name: 'Question',
    component: Question,
  },
]

const router = createRouter({
  history: createWebHistory(),
  routes,
})

export default router