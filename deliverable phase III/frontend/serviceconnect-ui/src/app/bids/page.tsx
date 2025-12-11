'use client';

import { useEffect, useState } from 'react';
import { bidsAPI } from '@/lib/api';

export default function BidsPage() {
  const [bids, setBids] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    try {
      setLoading(true);
      const res = await bidsAPI.getAll();
      setBids(res.data);
    } catch (err) {
      console.error('Error loading bids', err);
      alert('Error loading bids. Please check if the backend is running.');
    } finally {
      setLoading(false);
    }
  };

  if (loading) return <div className="text-center py-8">Loading bids...</div>;

  return (
    <div>
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-3xl font-bold">Bids</h1>
        <p className="text-sm text-gray-500">Showing latest 100</p>
      </div>

      {bids.length === 0 ? (
        <div className="card text-center">No bids found.</div>
      ) : (
        <div className="grid gap-4">
          {bids.map((b) => (
            <div key={b.bidID} className="card">
              <div className="flex justify-between items-start">
                <div>
                  <h3 className="text-xl font-semibold">Job: {b.jobTitle}</h3>
                  <p className="text-gray-600 text-sm">Worker: {b.workerName}</p>
                  <p className="text-gray-500 text-sm">Status: {b.status}</p>
                </div>
                <div className="text-right text-sm text-gray-600">
                  <div>Amount: <span className="font-semibold">${b.bidAmount}</span></div>
                  <div>Duration: <span className="font-semibold">{b.estimatedDuration ?? '—'} mins</span></div>
                  <div>Date: <span className="font-semibold">{new Date(b.bidDate).toLocaleDateString()}</span></div>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
