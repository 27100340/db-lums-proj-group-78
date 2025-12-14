import axios from 'axios';

const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5000/api';

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Job API
export const jobsAPI = {
  getAll: () => api.get('/jobs'),
  getById: (id: number) => api.get(`/jobs/${id}`),
  getOpen: () => api.get('/jobs/open'),
  getByCategory: (categoryId: number) => api.get(`/jobs/category/${categoryId}`),
  getByCustomer: (customerId: number) => api.get(`/jobs/customer/${customerId}`),
  getByLocation: (city: string, categoryId: number) => api.get(`/jobs/location/${city}/category/${categoryId}`),
  getActiveWithBids: () => api.get('/jobs/active-with-bids'),
  create: (job: any) => api.post('/jobs', job),
  update: (id: number, job: any) => api.put(`/jobs/${id}`, job),
  delete: (id: number) => api.delete(`/jobs/${id}`),
};

// Worker API
export const workersAPI = {
  getAll: () => api.get('/workers'),
  getById: (id: number) => api.get(`/workers/${id}`),
  getBySkill: (categoryId: number) => api.get(`/workers/skill/${categoryId}`),
  getByCity: (city: string) => api.get(`/workers/city/${city}`),
  getAvailable: (jobId: number, categoryId: number) => api.get(`/workers/available/${jobId}/category/${categoryId}`),
  getPerformance: (id: number) => api.get(`/workers/${id}/performance`),
  getTopPerformers: (categoryId: number) => api.get(`/workers/top-performers/category/${categoryId}`),
  getReliability: (id: number) => api.get(`/workers/${id}/reliability`),
  getTopRated: () => api.get('/workers/top-rated'),
  create: (worker: any) => api.post('/workers', worker),
  update: (id: number, worker: any) => api.put(`/workers/${id}`, worker),
  delete: (id: number) => api.delete(`/workers/${id}`),
};

// Customer API
export const customersAPI = {
  getAll: () => api.get('/customers'),
  getById: (id: number) => api.get(`/customers/${id}`),
  getByCity: (city: string) => api.get(`/customers/city/${city}`),
  getAnalytics: () => api.get('/customers/analytics'),
  create: (customer: any) => api.post('/customers', customer),
  update: (id: number, customer: any) => api.put(`/customers/${id}`, customer),
  delete: (id: number) => api.delete(`/customers/${id}`),
};

// Bid API
export const bidsAPI = {
  getAll: () => api.get('/bids'),
  getById: (id: number) => api.get(`/bids/${id}`),
  getByJob: (jobId: number) => api.get(`/bids/job/${jobId}`),
  getByWorker: (workerId: number) => api.get(`/bids/worker/${workerId}`),
  getStats: (jobId: number) => api.get(`/bids/job/${jobId}/stats`),
  create: (bid: any) => api.post('/bids', bid),
  accept: (id: number) => api.post(`/bids/${id}/accept`),
  update: (id: number, bid: any) => api.put(`/bids/${id}`, bid),
  delete: (id: number) => api.delete(`/bids/${id}`),
};

// Booking API
export const bookingsAPI = {
  getAll: () => api.get('/bookings'),
  getById: (id: number) => api.get(`/bookings/${id}`),
  getByWorker: (workerId: number) => api.get(`/bookings/worker/${workerId}`),
  getByCustomer: (customerId: number) => api.get(`/bookings/customer/${customerId}`),
  getSummaryByCategory: () => api.get('/bookings/summary-by-category'),
  create: (booking: any) => api.post('/bookings', booking),
  complete: (id: number, notes: string) => api.post(`/bookings/${id}/complete`, notes),
  update: (id: number, booking: any) => api.put(`/bookings/${id}`, booking),
  delete: (id: number) => api.delete(`/bookings/${id}`),
};

// Service Category API
export const categoriesAPI = {
  getAll: () => api.get('/servicecategories'),
  getById: (id: number) => api.get(`/servicecategories/${id}`),
};

// Stats API
export const statsAPI = {
  getCounts: () => api.get('/stats/counts'),
};

// Config API
export const configAPI = {
  getBllType: () => api.get('/config/bll-type'),
  setBllType: (type: string) => api.post(`/config/bll-type/${type}`),
};

export default api;
