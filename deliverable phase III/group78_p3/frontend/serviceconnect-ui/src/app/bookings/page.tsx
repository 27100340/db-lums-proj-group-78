'use client';

import { useEffect, useState } from 'react';
import { bookingsAPI } from '@/lib/api';

export default function BookingsPage() {
  const [bookings, setBookings] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    try {
      setLoading(true);
      const res = await bookingsAPI.getAll();
      setBookings(res.data);
    } catch (err) {
      console.error('Error loading bookings', err);
      alert('Error loading bookings. Please check if the backend is running.');
    } finally {
      setLoading(false);
    }
  };

  if (loading) return <div className="text-center py-8">Loading bookings...</div>;

  return (
    <div>
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-3xl font-bold">Bookings</h1>
        <p className="text-sm text-gray-500">Showing latest 100</p>
      </div>

      {bookings.length === 0 ? (
        <div className="card text-center">No bookings found.</div>
      ) : (
        <div className="grid gap-4">
          {bookings.map((b) => (
            <div key={b.bookingID} className="card">
              <div className="flex justify-between items-start">
                <div>
                  <h3 className="text-xl font-semibold">{b.jobTitle}</h3>
                  <p className="text-gray-600 text-sm">Worker: {b.workerName}</p>
                  <p className="text-gray-600 text-sm">Customer: {b.customerName}</p>
                </div>
                <div className="text-right text-sm text-gray-600">
                  <div>Status: <span className="font-semibold">{b.status}</span></div>
                  <div>Start: <span className="font-semibold">{b.scheduledStart ? new Date(b.scheduledStart).toLocaleString() : '—'}</span></div>
                  <div>End: <span className="font-semibold">{b.scheduledEnd ? new Date(b.scheduledEnd).toLocaleString() : '—'}</span></div>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
