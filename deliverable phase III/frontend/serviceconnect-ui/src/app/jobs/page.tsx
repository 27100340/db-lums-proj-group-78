'use client';

import { useEffect, useState } from 'react';
import { jobsAPI, categoriesAPI } from '@/lib/api';
import Link from 'next/link';

export default function JobsPage() {
  const [jobs, setJobs] = useState<any[]>([]);
  const [categories, setCategories] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  const [filter, setFilter] = useState('all');

  useEffect(() => {
    loadData();
  }, [filter]);

  const loadData = async () => {
    try {
      setLoading(true);
      const [jobsRes, categoriesRes] = await Promise.all([
        filter === 'open' ? jobsAPI.getOpen() : jobsAPI.getAll(),
        categoriesAPI.getAll()
      ]);
      setJobs(jobsRes.data);
      setCategories(categoriesRes.data);
    } catch (error) {
      console.error('Error loading data:', error);
      alert('Error loading jobs. Please check if the backend is running.');
    } finally {
      setLoading(false);
    }
  };

  const deleteJob = async (id: number) => {
    if (!confirm('Are you sure you want to delete this job?')) return;

    try {
      await jobsAPI.delete(id);
      setJobs(jobs.filter(j => j.jobID !== id));
      alert('Job deleted successfully!');
    } catch (error) {
      console.error('Error deleting job:', error);
      alert('Error deleting job');
    }
  };

  if (loading) {
    return <div className="text-center py-8">Loading jobs...</div>;
  }

  return (
    <div>
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-3xl font-bold">Jobs</h1>
        <div className="space-x-2">
          <button
            onClick={() => setFilter('all')}
            className={`btn ${filter === 'all' ? 'btn-primary' : 'btn-secondary'}`}
          >
            All Jobs
          </button>
          <button
            onClick={() => setFilter('open')}
            className={`btn ${filter === 'open' ? 'btn-primary' : 'btn-secondary'}`}
          >
            Open Jobs
          </button>
          <Link href="/jobs/create" className="btn btn-primary">
            + Create Job
          </Link>
        </div>
      </div>

      {jobs.length === 0 ? (
        <div className="card text-center">
          <p className="text-gray-500">No jobs found</p>
        </div>
      ) : (
        <div className="grid gap-4">
          {jobs.map((job) => (
            <div key={job.jobID} className="card hover:shadow-lg transition-shadow">
              <div className="flex justify-between items-start">
                <div className="flex-1">
                  <div className="flex items-center gap-2 mb-2">
                    <h3 className="text-xl font-semibold">{job.title}</h3>
                    <span className={`px-2 py-1 rounded text-xs font-semibold ${
                      job.status === 'Open' ? 'bg-green-100 text-green-800' :
                      job.status === 'Assigned' ? 'bg-blue-100 text-blue-800' :
                      job.status === 'Completed' ? 'bg-gray-100 text-gray-800' :
                      'bg-yellow-100 text-yellow-800'
                    }`}>
                      {job.status}
                    </span>
                    {job.urgencyLevel && (
                      <span className={`px-2 py-1 rounded text-xs font-semibold ${
                        job.urgencyLevel === 'Urgent' ? 'bg-red-100 text-red-800' :
                        job.urgencyLevel === 'High' ? 'bg-orange-100 text-orange-800' :
                        'bg-gray-100 text-gray-600'
                      }`}>
                        {job.urgencyLevel}
                      </span>
                    )}
                  </div>

                  <p className="text-gray-600 mb-2">{job.description}</p>

                  <div className="grid grid-cols-2 md:grid-cols-4 gap-2 text-sm">
                    <div>
                      <span className="text-gray-500">Customer:</span>
                      <span className="ml-1 font-medium">{job.customerName}</span>
                    </div>
                    <div>
                      <span className="text-gray-500">Category:</span>
                      <span className="ml-1 font-medium">{job.categoryName}</span>
                    </div>
                    <div>
                      <span className="text-gray-500">Budget:</span>
                      <span className="ml-1 font-medium">${job.budget}</span>
                    </div>
                    <div>
                      <span className="text-gray-500">Posted:</span>
                      <span className="ml-1">{new Date(job.postedDate).toLocaleDateString()}</span>
                    </div>
                  </div>

                  {job.location && (
                    <div className="mt-2 text-sm text-gray-600">
                      📍 {job.location}
                    </div>
                  )}
                </div>

                <div className="flex flex-col gap-2 ml-4">
                  <Link href={`/jobs/${job.jobID}`} className="btn btn-secondary text-sm">
                    View Details
                  </Link>
                  <Link href={`/bids?jobId=${job.jobID}`} className="btn btn-secondary text-sm">
                    View Bids
                  </Link>
                  <button
                    onClick={() => deleteJob(job.jobID)}
                    className="btn bg-red-500 text-white hover:bg-red-600 text-sm"
                  >
                    Delete
                  </button>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}

      <div className="mt-8 card bg-gray-50">
        <h2 className="font-bold mb-2">Service Categories</h2>
        <div className="flex flex-wrap gap-2">
          {categories.map((cat) => (
            <button
              key={cat.categoryID}
              onClick={() => {
                jobsAPI.getByCategory(cat.categoryID).then(res => setJobs(res.data));
              }}
              className="px-3 py-1 bg-white border border-gray-300 rounded hover:bg-gray-100"
            >
              {cat.categoryName}
            </button>
          ))}
        </div>
      </div>
    </div>
  );
}