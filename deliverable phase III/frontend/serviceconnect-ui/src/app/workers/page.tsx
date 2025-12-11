'use client';

import { useEffect, useState } from 'react';
import { workersAPI } from '@/lib/api';

export default function WorkersPage() {
  const [workers, setWorkers] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    try {
      setLoading(true);
      const res = await workersAPI.getAll();
      setWorkers(res.data);
    } catch (err) {
      console.error('Error loading workers', err);
      alert('Error loading workers. Please check if the backend is running.');
    } finally {
      setLoading(false);
    }
  };

  if (loading) return <div className="text-center py-8">Loading workers...</div>;

  return (
    <div>
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-3xl font-bold">Workers</h1>
        <p className="text-sm text-gray-500">Showing latest 100</p>
      </div>

      {workers.length === 0 ? (
        <div className="card text-center">No workers found.</div>
      ) : (
        <div className="grid gap-4">
          {workers.map((w) => (
            <div key={w.workerID} className="card">
              <div className="flex justify-between items-center">
                <div>
                  <h3 className="text-xl font-semibold">{w.firstName} {w.lastName}</h3>
                  <p className="text-gray-500 text-sm">{w.email}</p>
                </div>
                <div className="text-right text-sm text-gray-600">
                  <div>City: <span className="font-semibold">{w.city || 'N/A'}</span></div>
                  <div>Jobs: <span className="font-semibold">{w.totalJobsCompleted}</span></div>
                  <div>Rating: <span className="font-semibold">{w.overallRating ?? 'N/A'}</span></div>
                  <div>Rate: <span className="font-semibold">${w.hourlyRate ?? '—'}</span></div>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
