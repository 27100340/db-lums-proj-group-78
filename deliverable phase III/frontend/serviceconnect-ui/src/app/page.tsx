'use client';

import Link from 'next/link';
import { useEffect, useState } from 'react';
import { categoriesAPI, configAPI, statsAPI } from '@/lib/api';

export default function Home() {
  const [bllType, setBllType] = useState<string>('Loading...');
  const [categories, setCategories] = useState<any[]>([]);
  const [counts, setCounts] = useState<any | null>(null);

  useEffect(() => {
    // Fetch current BLL type
    configAPI.getBllType()
      .then(res => setBllType(res.data.bllType))
      .catch(() => setBllType('Unknown'));

    // Fetch service categories
    categoriesAPI.getAll()
      .then(res => setCategories(res.data))
      .catch(err => console.error('Error fetching categories:', err));

    // Fetch DB counts
    statsAPI.getCounts()
      .then(res => setCounts(res.data))
      .catch(err => console.error('Error fetching counts:', err));
  }, []);

  return (
    <div className="space-y-8">
      <div className="text-center py-12">
        <h1 className="text-5xl font-bold text-primary-700 mb-4">
          Welcome to ServiceConnect
        </h1>
        <p className="text-xl text-gray-600 mb-2">
          Phase 3: Application Development group 78
          Baqir , Muhammad , Ayma , Mustafa
        </p>
        <p className="text-lg text-gray-500">
          Current BLL Implementation: <span className="font-semibold text-primary-600">{bllType}</span>
        </p>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        <Link href="/jobs" className="card hover:shadow-lg transition-shadow">
          <h2 className="text-2xl font-bold text-primary-700 mb-2">Jobs</h2>
          <p className="text-gray-600">Browse and post service jobs</p>
        </Link>

        <Link href="/workers" className="card hover:shadow-lg transition-shadow">
          <h2 className="text-2xl font-bold text-primary-700 mb-2">Workers</h2>
          <p className="text-gray-600">Find skilled service workers</p>
        </Link>

        <Link href="/customers" className="card hover:shadow-lg transition-shadow">
          <h2 className="text-2xl font-bold text-primary-700 mb-2">Customers</h2>
          <p className="text-gray-600">Manage customer profiles</p>
        </Link>

        <Link href="/bids" className="card hover:shadow-lg transition-shadow">
          <h2 className="text-2xl font-bold text-primary-700 mb-2">Bids</h2>
          <p className="text-gray-600">View and manage job bids</p>
        </Link>

        <Link href="/bookings" className="card hover:shadow-lg transition-shadow">
          <h2 className="text-2xl font-bold text-primary-700 mb-2">Bookings</h2>
          <p className="text-gray-600">Track confirmed bookings</p>
        </Link>

        <Link href="/analytics" className="card hover:shadow-lg transition-shadow">
          <h2 className="text-2xl font-bold text-primary-700 mb-2">Analytics</h2>
          <p className="text-gray-600">View platform analytics</p>
        </Link>
      </div>

      <div className="card">
        <h2 className="text-2xl font-bold mb-4">Service Categories</h2>
        <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
          {categories.map((category) => (
            <div key={category.categoryID} className="p-4 bg-gray-50 rounded-lg">
              <h3 className="font-semibold">{category.categoryName}</h3>
              <p className="text-sm text-gray-600">${category.baseRate}/hr</p>
            </div>
          ))}
        </div>
      </div>

      {counts && (
        <div className="card">
          <h2 className="text-2xl font-bold mb-4">Database Snapshot</h2>
          <div className="grid grid-cols-2 md:grid-cols-4 gap-4 text-sm">
            <div><span className="text-gray-500">Users:</span> <span className="font-semibold">{counts.users}</span></div>
            <div><span className="text-gray-500">Workers:</span> <span className="font-semibold">{counts.workers}</span></div>
            <div><span className="text-gray-500">Customers:</span> <span className="font-semibold">{counts.customers}</span></div>
            <div><span className="text-gray-500">Categories:</span> <span className="font-semibold">{counts.serviceCategories}</span></div>
            <div><span className="text-gray-500">Jobs:</span> <span className="font-semibold">{counts.jobs}</span></div>
            <div><span className="text-gray-500">Bids:</span> <span className="font-semibold">{counts.bids}</span></div>
            <div><span className="text-gray-500">Bookings:</span> <span className="font-semibold">{counts.bookings}</span></div>
            <div><span className="text-gray-500">Notifications:</span> <span className="font-semibold">{counts.notifications}</span></div>
          </div>
        </div>
      )}

      <div className="card bg-primary-50 border border-primary-200">
        <h2 className="text-2xl font-bold mb-4 text-primary-800">Project Features</h2>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <h3 className="font-semibold text-primary-700 mb-2">BLL Implementations:</h3>
            <ul className="list-disc list-inside space-y-1 text-gray-700">
              <li>LINQ & Entity Framework Core</li>
              <li>Stored Procedures with ADO.NET</li>
              <li>Factory Pattern for runtime selection</li>
            </ul>
          </div>
          <div>
            <h3 className="font-semibold text-primary-700 mb-2">SQL Server Features:</h3>
            <ul className="list-disc list-inside space-y-1 text-gray-700">
              <li>Stored Procedures (6)</li>
              <li>Functions (4 - Scalar & Table-Valued)</li>
              <li>Triggers (5 - AFTER & INSTEAD OF)</li>
              <li>Views (4)</li>
              <li>CTEs (2)</li>
              <li>Indexes (15+)</li>
              <li>Table Partitioning</li>
            </ul>
          </div>
        </div>
      </div>
    </div>
  );
}
