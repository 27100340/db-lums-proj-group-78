'use client';

import { useEffect, useState } from 'react';
import { customersAPI } from '@/lib/api';

export default function CustomersPage() {
  const [customers, setCustomers] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    try {
      setLoading(true);
      const res = await customersAPI.getAll();
      setCustomers(res.data);
    } catch (err) {
      console.error('Error loading customers', err);
      alert('Error loading customers. Please check if the backend is running.');
    } finally {
      setLoading(false);
    }
  };

  if (loading) return <div className="text-center py-8">Loading customers...</div>;

  return (
    <div>
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-3xl font-bold">Customers</h1>
        <p className="text-sm text-gray-500">Showing latest 100</p>
      </div>

      {customers.length === 0 ? (
        <div className="card text-center">No customers found.</div>
      ) : (
        <div className="grid gap-4">
          {customers.map((c) => (
            <div key={c.customerID} className="card flex justify-between items-start">
              <div>
                <h3 className="text-xl font-semibold">{c.firstName} {c.lastName}</h3>
                <p className="text-gray-500 text-sm">{c.email}</p>
                <p className="text-gray-600 text-sm">{c.city || 'N/A'}</p>
              </div>
              <div className="text-right text-sm text-gray-600">
                <div>Rating: <span className="font-semibold">{c.customerRating ?? 'N/A'}</span></div>
                <div>Jobs Posted: <span className="font-semibold">{c.totalJobsPosted}</span></div>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
