'use client';

import { useEffect, useState } from 'react';
import { configAPI } from '@/lib/api';

export default function SettingsPage() {
  const [currentBllType, setCurrentBllType] = useState<string>('Loading...');
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    loadCurrentBllType();
  }, []);

  const loadCurrentBllType = async () => {
    try {
      const response = await configAPI.getBllType();
      setCurrentBllType(response.data.bllType);
    } catch (error) {
      console.error('Error loading BLL type:', error);
      setCurrentBllType('Error loading');
    }
  };

  const switchBllType = async (type: string) => {
    if (!confirm(`Are you sure you want to switch to ${type}? This will affect all subsequent API calls.`)) {
      return;
    }

    setLoading(true);
    try {
      await configAPI.setBllType(type);
      setCurrentBllType(type);
      alert(`Successfully switched to ${type} implementation!`);
      window.location.reload(); // Reload to see changes
    } catch (error) {
      console.error('Error switching BLL type:', error);
      alert('Error switching BLL type');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="max-w-4xl mx-auto">
      <h1 className="text-3xl font-bold mb-6">Settings</h1>

      <div className="card mb-6">
        <h2 className="text-2xl font-bold mb-4">Business Logic Layer (BLL) Implementation</h2>

        <div className="mb-6">
          <p className="text-gray-600 mb-2">
            Current Implementation: <span className="font-bold text-primary-600 text-lg">{currentBllType}</span>
          </p>
          <p className="text-sm text-gray-500">
            Switch between LINQ/Entity Framework and Stored Procedure implementations at runtime using the Factory Pattern.
          </p>
        </div>

        <div className="grid md:grid-cols-2 gap-4">
          <div className={`p-6 border-2 rounded-lg ${currentBllType === 'LinqEF' ? 'border-primary-500 bg-primary-50' : 'border-gray-300'}`}>
            <h3 className="text-xl font-bold mb-2">LINQ / Entity Framework</h3>
            <p className="text-sm text-gray-600 mb-4">
              Uses LINQ queries and Entity Framework Core for data access. Provides type-safe queries and automatic change tracking.
            </p>
            <ul className="text-sm text-gray-700 space-y-1 mb-4">
              <li>✓ Type-safe LINQ queries</li>
              <li>✓ EF Core change tracking</li>
              <li>✓ Automatic SQL generation</li>
              <li>✓ Navigation properties</li>
            </ul>
            <button
              onClick={() => switchBllType('LinqEF')}
              disabled={loading || currentBllType === 'LinqEF'}
              className={`btn w-full ${currentBllType === 'LinqEF' ? 'btn-secondary cursor-not-allowed' : 'btn-primary'}`}
            >
              {currentBllType === 'LinqEF' ? 'Currently Active' : 'Switch to LINQ/EF'}
            </button>
          </div>

          <div className={`p-6 border-2 rounded-lg ${currentBllType === 'StoredProcedure' ? 'border-primary-500 bg-primary-50' : 'border-gray-300'}`}>
            <h3 className="text-xl font-bold mb-2">Stored Procedures</h3>
            <p className="text-sm text-gray-600 mb-4">
              Uses ADO.NET to call stored procedures, functions, views, and execute SQL commands directly. Optimized database performance.
            </p>
            <ul className="text-sm text-gray-700 space-y-1 mb-4">
              <li>✓ Direct SP execution</li>
              <li>✓ Optimized performance</li>
              <li>✓ Complex business logic</li>
              <li>✓ Transaction management</li>
            </ul>
            <button
              onClick={() => switchBllType('StoredProcedure')}
              disabled={loading || currentBllType === 'StoredProcedure'}
              className={`btn w-full ${currentBllType === 'StoredProcedure' ? 'btn-secondary cursor-not-allowed' : 'btn-primary'}`}
            >
              {currentBllType === 'StoredProcedure' ? 'Currently Active' : 'Switch to Stored Procedures'}
            </button>
          </div>
        </div>
      </div>

      <div className="card">
        <h2 className="text-xl font-bold mb-4">SQL Server Features Utilized</h2>
        <div className="grid md:grid-cols-2 gap-4">
          <div>
            <h3 className="font-semibold text-primary-700 mb-2">Stored Procedures (6)</h3>
            <ul className="text-sm space-y-1 text-gray-700">
              <li>• sp_AcceptBid</li>
              <li>• sp_CompleteBooking</li>
              <li>• sp_GetAvailableWorkers</li>
              <li>• sp_GetWorkerPerformance</li>
              <li>• sp_TopPerformersByCategory</li>
              <li>• sp_ComplexJobAnalysis</li>
            </ul>
          </div>

          <div>
            <h3 className="font-semibold text-primary-700 mb-2">Functions (4)</h3>
            <ul className="text-sm space-y-1 text-gray-700">
              <li>• fn_CalculateJobComplexity</li>
              <li>• fn_GetWorkerReliabilityScore</li>
              <li>• fn_GetJobsByLocation</li>
              <li>• fn_GetBidStats</li>
            </ul>
          </div>

          <div>
            <h3 className="font-semibold text-primary-700 mb-2">Triggers (5)</h3>
            <ul className="text-sm space-y-1 text-gray-700">
              <li>• trg_UpdateWorkerRatingOnReview</li>
              <li>• trg_NotifyOnBidAccepted</li>
              <li>• trg_UpdateJobCompletionOnBooking</li>
              <li>• trg_PreventDeleteCompletedBooking</li>
              <li>• trg_ValidateBidAmount</li>
            </ul>
          </div>

          <div>
            <h3 className="font-semibold text-primary-700 mb-2">Views (4)</h3>
            <ul className="text-sm space-y-1 text-gray-700">
              <li>• vw_ActiveJobsWithBids</li>
              <li>• vw_TopRatedWorkers</li>
              <li>• vw_BookingSummaryByCategory</li>
              <li>• vw_CustomerAnalytics</li>
            </ul>
          </div>
        </div>

        <div className="mt-4 p-4 bg-gray-50 rounded">
          <p className="text-sm text-gray-600">
            <strong>Note:</strong> All SQL Server features from Phase 2 are integrated into both BLL implementations.
            The Factory Pattern allows seamless switching between implementations without changing the frontend code.
          </p>
        </div>
      </div>
    </div>
  );
}