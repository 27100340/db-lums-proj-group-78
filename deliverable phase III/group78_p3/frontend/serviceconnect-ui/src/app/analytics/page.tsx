'use client';

import { useEffect, useState } from 'react';
import { statsAPI, bookingsAPI, workersAPI } from '@/lib/api';

export default function AnalyticsPage() {
  const [counts, setCounts] = useState<any | null>(null);
  const [bookingSummary, setBookingSummary] = useState<any[]>([]);
  const [topWorkers, setTopWorkers] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    try {
      setLoading(true);
      const [countsRes, summaryRes, workersRes] = await Promise.all([
        statsAPI.getCounts(),
        bookingsAPI.getSummaryByCategory(),
        workersAPI.getTopRated()
      ]);
      setCounts(countsRes.data);
      setBookingSummary(summaryRes.data);
      setTopWorkers(workersRes.data.slice(0, 10));
    } catch (err) {
      console.error('Error loading analytics', err);
      alert('Error loading analytics. Please check if the backend is running.');
    } finally {
      setLoading(false);
    }
  };

  if (loading) return <div className="text-center py-8">Loading analytics...</div>;

  return (
    <div className="space-y-6">
      <h1 className="text-3xl font-bold">Analytics</h1>

      {counts && (
        <div className="card">
          <h2 className="text-2xl font-bold mb-4">Platform Counts</h2>
          <div className="grid grid-cols-2 md:grid-cols-4 gap-4 text-sm">
            <div><span className="text-gray-500">Users:</span> <span className="font-semibold">{counts.users}</span></div>
            <div><span className="text-gray-500">Workers:</span> <span className="font-semibold">{counts.workers}</span></div>
            <div><span className="text-gray-500">Customers:</span> <span className="font-semibold">{counts.customers}</span></div>
            <div><span className="text-gray-500">Jobs:</span> <span className="font-semibold">{counts.jobs}</span></div>
            <div><span className="text-gray-500">Bids:</span> <span className="font-semibold">{counts.bids}</span></div>
            <div><span className="text-gray-500">Bookings:</span> <span className="font-semibold">{counts.bookings}</span></div>
            <div><span className="text-gray-500">Reviews:</span> <span className="font-semibold">{counts.reviews}</span></div>
            <div><span className="text-gray-500">Notifications:</span> <span className="font-semibold">{counts.notifications}</span></div>
          </div>
        </div>
      )}

      <div className="card">
        <h2 className="text-2xl font-bold mb-4">Booking Summary by Category</h2>
        {bookingSummary.length === 0 ? (
          <p className="text-gray-500 text-sm">No booking data.</p>
        ) : (
          <div className="overflow-x-auto">
            <table className="min-w-full text-sm">
              <thead>
                <tr className="text-left border-b">
                  <th className="py-2 pr-4">Category</th>
                  <th className="py-2 pr-4">Scheduled</th>
                  <th className="py-2 pr-4">In Progress</th>
                  <th className="py-2 pr-4">Completed</th>
                  <th className="py-2 pr-4">Cancelled</th>
                  <th className="py-2 pr-4">Total</th>
                </tr>
              </thead>
              <tbody>
                {bookingSummary.map((row) => (
                  <tr key={row.categoryName} className="border-b last:border-0">
                    <td className="py-2 pr-4">{row.categoryName}</td>
                    <td className="py-2 pr-4">{row.scheduledCount}</td>
                    <td className="py-2 pr-4">{row.inProgressCount}</td>
                    <td className="py-2 pr-4">{row.completedCount}</td>
                    <td className="py-2 pr-4">{row.cancelledCount}</td>
                    <td className="py-2 pr-4 font-semibold">{row.totalBookings}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>

      <div className="card">
        <h2 className="text-2xl font-bold mb-4">Top Rated Workers (Top 10)</h2>
        {topWorkers.length === 0 ? (
          <p className="text-gray-500 text-sm">No data.</p>
        ) : (
          <ul className="space-y-2 text-sm">
            {topWorkers.map((w) => (
              <li key={w.workerID} className="flex justify-between">
                <span>{w.fullName}</span>
                <span className="text-gray-600">Rating: <span className="font-semibold">{w.overallRating ?? 'N/A'}</span></span>
              </li>
            ))}
          </ul>
        )}
      </div>
    </div>
  );
}
